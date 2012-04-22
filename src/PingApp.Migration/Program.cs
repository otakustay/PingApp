using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using PingApp.Entity;

namespace PingApp.Migration {
    class Program {
        private const string SELECT_COMMAND_TEXT =
@"select
    a.Id,
    a.Description,
    a.LargeIconUrl,
    a.SellerName,
    a.SellerViewUrl,
    a.ReleaseNotes,
    a.CensoredName,
    a.ContentRating,
    a.ContentAdvisoryRating,
    a.AverageUserRating,
    a.UserRatingCount,
    a.Languages,
    a.Categories,
    a.ScreenshotUrls,
    a.IPadScreenshotUrls,
    b.Name,
    b.Version,
    b.ReleaseDate,
    b.Price,
    b.Currency,
    b.FileSize,
    b.ViewUrl,
    b.IconUrl,
    b.Introduction,
    b.ReleaseNotes,
    b.PrimaryCategory,
    b.DeveloperId,
    b.DeveloperName,
    b.DeveloperViewUrl,
    b.AverageUserRatingForCurrentVersion,
    b.UserRatingCountForCurrentVersion,
    b.Features,
    b.SupportedDevices,
    b.LastValidUpdateTime,
    b.LastValidUpdateType,
    b.LastValidUpdateOldValue,
    b.LastValidUpdateNewValue,
    b.LanguagePriority,
    b.IsValid
from AppBrief b inner join App a on a.id = b.id
where b.id in ({0});";

        private static readonly MongoCollection<App> apps;

        private static readonly MongoCollection<RevokedApp> revokedApps;

        private static readonly MongoCollection<AppUpdate> appUpdates;

        private static readonly MongoCollection<AppTrack> appTracks;

        private static readonly MongoCollection<User> users;

        private static readonly MySqlConnection connection;

        static Program() {
            connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySql"].ConnectionString);
            connection.Open();

            var server = MongoServer.Create(ConfigurationManager.ConnectionStrings["Mongo"].ConnectionString);
            var database = server.GetDatabase("pingapp");
            apps = database.GetCollection<App>("apps");
            revokedApps = database.GetCollection<RevokedApp>("revokedApps");
            appUpdates = database.GetCollection<AppUpdate>("appUpdates");
            appTracks = database.GetCollection<AppTrack>("appTracks");
            users = database.GetCollection<User>("users");
        }

        static void Main(string[] args) {
            int count;
            int offset = 0;
            int batchSize = 800;

            Console.WriteLine("Migrating apps...");
            do {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                count = MigrateApps(offset, batchSize);

                watch.Stop();
                Console.WriteLine("{0} + {1} : {2}", offset, count, watch.ElapsedMilliseconds);

                offset += batchSize;
            }
            while (count >= batchSize);
            Console.WriteLine("Apps migrated");

            offset = 0;
            Console.WriteLine("Migrating updates...");
            do {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                count = MigrateAppUpdates(offset, batchSize);

                watch.Stop();
                Console.WriteLine("{0} + {1} : {2}", offset, count, watch.ElapsedMilliseconds);

                offset += batchSize;
            }
            while (count >= batchSize);
            Console.WriteLine("Updates migrated");
        }

        private static int MigrateApps(int offset, int batchSize) {
            /*
             * 1. 获取App信息
             * 2. 如果IsValid是0表示已经下架：
             *    2.1. 从AppUpdate中找出最后类型为下架的更新，RevokedTime为该更新的时间
             *    2.2. 放入RevokedApp中
             * 3. 如果IsValid是1，放入App中
             */
            ICollection<int> identities = RetrieveIdentities(offset, batchSize);

            MySqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = String.Format(SELECT_COMMAND_TEXT, String.Join(",", identities));
            int count = 0;

            List<App> active = new List<App>();
            List<RevokedApp> revoked = new List<RevokedApp>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            using (IDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    App app = new App() {
                        Id = reader.GetInt32(0),
                        Description = reader.GetString(1),
                        LargeIconUrl = reader.GetString(2),
                        Seller = new Seller() {
                            Name = reader.GetString(3),
                            ViewUrl = reader.GetString(4)
                        },
                        ReleaseNotes = reader.GetString(5),
                        CensoredName = reader.GetString(6),
                        ContentRating = reader.GetString(7),
                        ContentAdvisoryRating = reader.GetString(8),
                        AverageUserRating = reader.IsDBNull(9) ? (float?)null : reader.GetFloat(9),
                        UserRatingCount = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                        Languages = reader.GetString(11).Split(','),
                        Categories = reader.GetString(12).Split(',').Select(i => Category.Get(Convert.ToInt32(i))).ToArray(),
                        ScreenshotUrls = reader.GetString(13).Split(','),
                        IPadScreenshotUrls = reader.GetString(14).Split(','),
                        Brief = new AppBrief() {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(15),
                            Version = reader.GetString(16),
                            ReleaseDate = reader.GetDateTime(17),
                            Price = reader.GetFloat(18),
                            Currency = reader.GetString(19),
                            FileSize = reader.GetInt32(20),
                            ViewUrl = reader.GetString(21),
                            IconUrl = reader.GetString(22),
                            Introduction = reader.GetString(23),
                            ReleaseNotes = reader.GetString(24),
                            PrimaryCategory = Category.Get(reader.GetInt32(25)),
                            Developer = new Developer() {
                                Id = reader.GetInt32(26),
                                Name = reader.GetString(27),
                                ViewUrl = reader.GetString(28)
                            },
                            AverageUserRatingForCurrentVersion = reader.IsDBNull(29) ? (float?)null : reader.GetFloat(29),
                            UserRatingCountForCurrentVersion = reader.IsDBNull(30) ? (int?)null : reader.GetInt32(30),
                            Features = reader.GetString(31).Split(','),
                            SupportedDevices = reader.GetString(32).Split(','),
                            LastValidUpdate = new AppUpdate() {
                                Time = reader.GetDateTime(33),
                                Type = (AppUpdateType)reader.GetInt32(34),
                                OldValue = reader.GetString(35),
                                NewValue = reader.GetString(36)
                            },
                            LanguagePriority = reader.GetInt32(37)
                        }
                    };

                    if (reader.GetByte(38) == 0) {
                        revoked.Add(new RevokedApp(app));
                    }
                    else {
                        active.Add(app);
                    }

                    count++;
                }
            }

            watch.Stop();
            Console.WriteLine("{0} active, {1} revoked, {2}", active.Count, revoked.Count, watch.Elapsed);

            watch.Reset();
            watch.Start();

            foreach (var app in revoked) {
                AppUpdate update = GetRevokeUpdate(app.Id);
                Trace.Assert(update.Type == AppUpdateType.Revoke, "update type check");
                app.RevokeTime = update.Time;
            }

            watch.Stop();
            Console.WriteLine("Retrive {0} revoke time using {1}", revoked.Count, watch.Elapsed);

            watch.Reset();
            watch.Start();

            apps.InsertBatch(active);
            revokedApps.InsertBatch(revoked);

            watch.Stop();
            Console.WriteLine("Saved to mongo using {0}", watch.Elapsed);

            return count;
        }

        private static int MigrateAppUpdates(int offset, int batchSize) {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "select App, Time, Type, OldValue, NewValue from AppUpdate limit ?offset, ?batchSize";
            command.Parameters.AddWithValue("?offset", offset);
            command.Parameters.AddWithValue("?batchSize", batchSize);

            List<AppUpdate> updates = new List<AppUpdate>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            using (var reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    AppUpdate update = new AppUpdate() {
                        App = reader.GetInt32(0),
                        Time = reader.GetDateTime(1),
                        Type = (AppUpdateType)reader.GetInt32(2),
                        OldValue = reader.GetString(3),
                        NewValue = reader.GetString(4)
                    };
                    updates.Add(update);
                }
            }

            watch.Stop();
            Console.WriteLine("Retrieved {0} updates using {1}", updates.Count, watch.Elapsed);

            watch.Reset();
            watch.Start();

            appUpdates.InsertBatch(updates);

            watch.Stop();
            Console.WriteLine("Saved to mongo using {0}", watch.Elapsed);

            return updates.Count;
        }

        private static ICollection<int> RetrieveIdentities(int offset, int batchSize) {
            MySqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "select Id from AppBrief limit ?offset, ?batchSize";
            command.Parameters.AddWithValue("?offset", offset);
            command.Parameters.AddWithValue("?batchSize", batchSize);
            ICollection<int> identities = new List<int>();
            using (IDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    int identity = reader.GetInt32(0);
                    identities.Add(identity);
                }
            }
            return identities;
        }

        private static AppUpdate GetRevokeUpdate(int app) {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "select Time, OldValue, NewValue from AppUpdate where App = ?app and Type = 6 order by Time desc limit 1";
            command.Parameters.AddWithValue("?app", app);
            using (var reader = command.ExecuteReader()) {
                reader.Read();
                AppUpdate update = new AppUpdate() {
                    App = app,
                    Time = reader.GetDateTime(0),
                    OldValue = reader.GetString(1),
                    NewValue = reader.GetString(2),
                    Type = AppUpdateType.Revoke
                };
                return update;
            }
        }
    }
}
