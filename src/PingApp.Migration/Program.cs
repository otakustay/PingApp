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
using MongoDB.Driver.Builders;
using MySql.Data.MySqlClient;
using Ninject;
using PingApp.Entity;
using PingApp.Repository.Mongo.Dependency;

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

        private static Dictionary<float, float> priceMapping = new Dictionary<float, float>() {
            { 0.99f, 6f }, { 1.99f, 12f }, { 2.99f, 18f }, { 3.99f, 25f }, { 4.99f, 30f }, { 5.99f, 40f },
            { 6.99f, 45f }, { 7.99f, 50f }, { 8.99f, 60f }, { 9.99f, 68f }, { 10.99f, 73f }, { 11.99f, 78f },
            { 12.99f, 88f }, { 13.99f, 93f }, { 14.99f, 98f }, { 15.99f, 108f }, { 16.99f, 113f }, { 17.99f, 118f },
            { 18.99f, 123f }, { 19.99f, 128f }, { 20.99f, 138f }, { 21.99f, 148f }, { 22.99f, 153f }, { 23.99f, 158f },
            { 24.99f, 163f }, { 25.99f, 168f }, { 26.99f, 178f }, { 27.99f, 188f }, { 28.99f, 193f }, { 29.99f, 198f },
            { 30.99f, 208f }, { 31.99f, 218f }, { 32.99f, 223f }, { 34.99f, 233f }, { 35.99f, 238f }, { 36.99f, 243f },
            { 37.99f, 248f }, { 38.99f, 253f }, { 39.99f, 258f }, { 40.99f, 263f }, { 41.99f, 268f }, { 43.99f, 278f },
            { 44.99f, 283f }, { 48.99f, 318f }, { 49.99f, 328f }, { 54.99f, 348f }, { 59.99f, 388f }, { 64.99f, 418f },
            { 69.99f, 448f }, { 74.99f, 488f }, { 79.99f, 518f }, { 84.99f, 548f }, { 89.99f, 588f }, { 94.99f, 618f },
            { 99.99f, 648f }, { 109.99f, 698f }, { 119.99f, 798f }, { 129.99f, 848f }, { 139.99f, 898f }, { 149.99f, 998f },
            { 169.99f, 1098f }, { 179.99f, 1198f }, { 199.99f, 1298f }, { 239.99f, 1598f }, { 249.99f, 1648f }, { 399.99f, 2598f },
            { 449.99f, 2998f }, { 499.99f, 3298f }, { 999.99f, 6498f }
        };

        private static readonly MongoCollection<App> apps;

        private static readonly MongoCollection<RevokedApp> revokedApps;

        private static readonly MongoCollection<AppUpdate> appUpdates;

        private static readonly MongoCollection<AppTrack> appTracks;

        private static readonly MongoCollection<User> users;

        private static readonly MySqlConnection connection;

        static Program() {
            connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySql"].ConnectionString);
            connection.Open();

            IKernel kernel = new StandardKernel();
            kernel.Load(new MongoRepositoryModule());

            apps = kernel.Get<MongoCollection<App>>();
            revokedApps = kernel.Get<MongoCollection<RevokedApp>>();
            appUpdates = kernel.Get<MongoCollection<AppUpdate>>();
            appTracks = kernel.Get<MongoCollection<AppTrack>>();
            users = kernel.Get<MongoCollection<User>>();
        }

        static void Main(string[] args) {
            if (args.Contains("updates")) {
                Console.WriteLine("Migrating updates...");
                DoWork(MigrateAppUpdates, 4000);
                Console.WriteLine("Updates migrated");
            }

            if (args.Contains("apps")) {
                Console.WriteLine("Migrating apps...");
                DoWork(MigrateApps);
                Console.WriteLine("Apps migrated");
            }
        }

        private static void DoWork(Func<int, int, int> action, int batchSize = 800) {
            int count;
            int offset = 0;

            do {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                count = action(offset, batchSize);

                watch.Stop();
                Console.WriteLine("{0} + {1} : {2}", offset, count, watch.ElapsedMilliseconds);

                offset += batchSize;
            }
            while (count >= batchSize);
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

                    AppUpdate storedUpdate = GetLastValidUpdate(app);
                    app.Brief.LastValidUpdate = storedUpdate;

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

                    // AppStore有一次把美元换成人民币，导致大量的“涨价”更新，特点是从浮点数变成整数，这一批要丢弃
                    if (update.Type == AppUpdateType.PriceIncrease) {
                        float oldPrice = Single.Parse(update.OldValue);
                        float newPrice = Single.Parse(update.NewValue);
                        if ((int)oldPrice != oldPrice /* 原价是浮点数 */ && (int)newPrice == oldPrice /* 现价是整数 */) {
                            continue;
                        }
                    }

                    // 新建和加入到应用的2个更新，在新系统中使用的是NewValue，需要换过来
                    if (update.Type == AppUpdateType.New || update.Type == AppUpdateType.AddToPing) {
                        update.NewValue = update.OldValue;
                        update.OldValue = String.Empty;
                    }

                    // 现有AppStore中国区全部是用人民币作为价格，因此将美元全部换回人民币
                    if (update.Type == AppUpdateType.PriceIncrease ||
                        update.Type == AppUpdateType.PriceDecrease ||
                        update.Type == AppUpdateType.PriceFree) {
                        // 价格相关的更新，OldValue和NewValue都是价格直接值
                        float oldPrice = Single.Parse(update.OldValue);
                        float newPrice = Single.Parse(update.NewValue);
                        if ((int)oldPrice != oldPrice) {
                            update.OldValue = priceMapping[oldPrice].ToString();
                        }
                        if ((int)newPrice != newPrice) {
                            update.NewValue = priceMapping[newPrice].ToString();
                        }
                    }
                    else if (update.Type == AppUpdateType.New ||
                        update.Type == AppUpdateType.AddToPing ||
                        update.Type == AppUpdateType.Revoke) {
                        // 另外几个更新，用的是{version, $price}的形式，需要分隔开来再计算
                        string[] oldValueParts = update.OldValue.Split(new string[] { ", " }, StringSplitOptions.None);
                        string[] newValueParts = update.NewValue.Split(new string[] { ", " }, StringSplitOptions.None);
                        float oldPrice = Single.Parse(oldValueParts[1]);
                        float newPrice = Single.Parse(newValueParts[1]);
                        if ((int)oldPrice != oldPrice) {
                            oldValueParts[1] = priceMapping[oldPrice].ToString();
                        }
                        if ((int)newPrice != newPrice) {
                            newValueParts[1] = priceMapping[newPrice].ToString();
                        }
                        update.OldValue = String.Join(", ", oldValueParts);
                        update.NewValue = String.Join(", ", newValueParts);
                    }
                    // 版本更新不需要动

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
            IMongoQuery query = Query.And(
                Query.EQ("app", app),
                Query.EQ("type", 6)
            );
            AppUpdate update = appUpdates.Find(query)
                .SetSortOrder(SortBy.Descending("time"))
                .SetLimit(1)
                .First();

            return update;
        }

        private static AppUpdate GetLastValidUpdate(App app) {
            IMongoQuery query = Query.EQ("app", app.Id);
            AppUpdate[] updates = appUpdates.Find(query)
                .SetSortOrder(SortBy.Descending("time"))
                .ToArray();

            // 当第一次加入应用时，App的最后更新类型为New，这是为了显示效果，但时间是AddToPing的时间，为了排序效果
            if (app.Brief.LastValidUpdate.Type == AppUpdateType.New) {
                AppUpdate update = updates.First(u => u.Type == AppUpdateType.AddToPing);
                return update;
            }

            // AppStore曾经将价格从美元改为人民币，这些数据

            throw new NotImplementedException();
        }
    }
}
