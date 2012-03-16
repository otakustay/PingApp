using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using MySql.Data.MySqlClient;
using System.Data;
using PingApp.Utility;
using System.Diagnostics;
using System.Configuration;

namespace PingApp.Schedule.Task {
    class ComputeHashTask : TaskNode {
        private MySqlConnection connection;

        private MySqlTransaction transaction;

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start compute hash for apps");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            IEnumerable<int> list = input.Get<IEnumerable<int>>();

            Buffer<int> buffer = new Buffer<int>(Program.BatchSize, apps => ComputeHashForApps(apps));

            foreach (int id in list) {
                buffer.Add(id);
            }
            buffer.Flush();

            watch.Stop();
            Log.Info("Work done using {0}:{1}", watch.Elapsed.Minutes, watch.Elapsed.Seconds);

            return null;
        }

        private void ComputeHashForApps(int[] list) {
            Log.Info("Try to retrieve {0} apps from db", list.Length);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            using (connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                using (transaction = connection.BeginTransaction()) {
                    try {
                        List<App> apps = GetApps(list);
                        watch.Stop();
                        Log.Info("{0} records retrieved from db using {1}ms", apps.Count, watch.ElapsedMilliseconds);
                        if (apps.Count != list.Length) {
                            Log.Warn("{0} records missed", apps.Count - list.Length);
                        }

                        watch.Start();

                        foreach (App app in apps) {
                            string hash = Utility.ComputeAppHash(app, 0);
                            MySqlCommand cmd = CreateCommand();
                            cmd.CommandText = "update AppBrief set Hash = ?Hash where Id = ?Id";
                            cmd.Parameters.AddWithValue("?Hash", hash);
                            cmd.Parameters.AddWithValue("?Id", app.Id);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        watch.Stop();
                        Log.Info("{0} apps processed using {1}ms", apps.Count, watch.ElapsedMilliseconds);
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.ErrorException("Update failed", ex);
                    }
                }
            }

            connection = null;
            transaction = null;
        }

        private MySqlCommand CreateCommand() {
            if (connection == null) {
                return null;
            }
            MySqlCommand cmd = connection.CreateCommand();
            if (transaction != null) {
                cmd.Transaction = transaction;
            }
            return cmd;
        }

        private List<App> GetApps(int[] list) {
            List<App> apps = new List<App>(list.Length);
            string sql =
@"select 
    a.Id `a.Id`, a.CensoredName `a.CensoredName`, a.Description `a.Description`, a.LargeIconUrl `a.LargeIconUrl`, 
    a.SellerName `a.SellerName`, a.SellerViewUrl `a.SellerViewUrl`, a.ReleaseNotes `a.ReleaseNotes`, 
    a.ContentAdvisoryRating `a.ContentAdvisoryRating`, a.ContentRating `a.ContentRating`, a.AverageUserRating `a.AverageUserRating`, 
    a.UserRatingCount `a.UserRatingCount`, a.Languages `a.Languages`, a.Categories `a.Categories`, 
    a.ScreenshotUrls `a.ScreenshotUrls`, a.IPadScreenshotUrls `a.IPadScreenshotUrls`, b.Id `b.Id`, b.DeveloperId `b.DeveloperId`, 
    b.DeveloperName `b.DeveloperName`, b.DeveloperViewUrl `b.DeveloperViewUrl`, b.Price `b.Price`, 
    b.Currency `b.Currency`, b.Version `b.Version`, b.ReleaseDate `b.ReleaseDate`, b.Name `b.Name`, b.Introduction `b.Introduction`, 
    b.ReleaseNotes `b.ReleaseNotes`, b.PrimaryCategory `b.PrimaryCategory`, b.ViewUrl `b.ViewUrl`, b.IconUrl `b.IconUrl`, 
    b.FileSize `b.FileSize`, b.AverageUserRatingForCurrentVersion `b.AverageUserRatingForCurrentVersion`, 
    b.UserRatingCountForCurrentVersion `b.UserRatingCountForCurrentVersion`, b.SupportedDevices `b.SupportedDevices`,
    b.Features `b.Features`, b.IsGameCenterEnabled `b.IsGameCenterEnabled`, b.DeviceType `b.DeviceType`, 
    b.LastValidUpdateTime `b.LastValidUpdateTime`, b.LastValidUpdateType `b.LastValidUpdateType`, 
    b.LastValidUpdateOldValue `b.LastValidUpdateOldValue`, b.LastValidUpdateNewValue `b.LastValidUpdateNewValue`, b.LanguagePriority `b.LanguagePriority`
from PingApp.AppBrief b
inner join PingApp.App a on a.Id = b.Id
where b.Id in ({0})";

            MySqlCommand cmd = CreateCommand();
            cmd.CommandText = String.Format(sql, String.Join(",", list));
            using (IDataReader reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    App app = new App() {
                        Id = reader.Get<int>("a.Id"),
                        CensoredName = reader.Get<string>("a.CensoredName"),
                        Description = reader.Get<string>("a.Description"),
                        ReleaseNotes = reader.Get<string>("a.ReleaseNotes"),
                        LargeIconUrl = reader.Get<string>("a.LargeIconUrl"),
                        ScreenshotUrls = reader.Get<string>("a.ScreenshotUrls")
                            .Split(',').Where(s => s.Length > 0).ToArray(),
                        IPadScreenshotUrls = reader.Get<string>("a.IPadScreenshotUrls")
                            .Split(',').Where(s => s.Length > 0).ToArray(),
                        ContentRating = reader.Get<string>("a.ContentRating"),
                        ContentAdvisoryRating = reader.Get<string>("a.ContentAdvisoryRating"),
                        AverageUserRating = reader.Get<float?>("a.AverageUserRating"),
                        UserRatingCount = reader.Get<int?>("a.UserRatingCount"),
                        Languages = reader.Get<string>("a.Languages")
                            .Split(',').Where(s => s.Length > 0).ToArray(),
                        Seller = new Seller() {
                            Name = reader.Get<string>("a.SellerName"),
                            ViewUrl = reader.Get<string>("a.SellerViewUrl")
                        },
                        Categories = reader.Get<string>("a.Categories").Split(',')
                            .Where(s => s.Length > 0)
                            .Select(c => Category.Get(Convert.ToInt32(c))).ToArray(),
                        Brief = new AppBrief() {
                            Id = reader.Get<int>("b.Id"),
                            Name = reader.Get<string>("b.Name"),
                            Version = reader.Get<string>("b.Version"),
                            ReleaseDate = reader.Get<DateTime>("b.ReleaseDate"),
                            Introduction = reader.Get<string>("b.Introduction"),
                            ReleaseNotes = reader.Get<string>("b.ReleaseNotes"),
                            Price = reader.Get<float>("b.Price"),
                            Currency = reader.Get<string>("b.Currency"),
                            FileSize = reader.Get<int>("b.FileSize"),
                            ViewUrl = reader.Get<string>("b.ViewUrl"),
                            IconUrl = reader.Get<string>("b.IconUrl"),
                            AverageUserRatingForCurrentVersion = reader.Get<float?>("b.AverageUserRatingForCurrentVersion"),
                            UserRatingCountForCurrentVersion = reader.Get<int?>("b.UserRatingCountForCurrentVersion"),
                            LanguagePriority = reader.Get<int>("b.LanguagePriority"),
                            Features = reader.Get<string>("b.Features")
                                .Split(',').Where(s => s.Length > 0).ToArray(),
                            SupportedDevices = reader.Get<string>("b.SupportedDevices")
                                .Split(',').Where(s => s.Length > 0).ToArray(),
                            PrimaryCategory = Category.Get(reader.Get<int>("b.PrimaryCategory")),
                            Developer = new Developer() {
                                Id = reader.Get<int>("b.DeveloperId"),
                                Name = reader.Get<string>("b.DeveloperName"),
                                ViewUrl = reader.Get<string>("b.DeveloperViewUrl")
                            },
                            LastValidUpdate = new AppUpdate() {
                                Time = reader.Get<DateTime>("b.LastValidUpdateTime"),
                                Type = reader.Get<AppUpdateType>("b.LastValidUpdateType"),
                                OldValue = reader.Get<string>("b.LastValidUpdateOldValue"),
                                NewValue = reader.Get<string>("b.LastValidUpdateNewValue"),
                                App = reader.Get<int>("b.Id")
                            }
                        }
                    };
                    apps.Add(app);
                }
            }

            return apps;
        }
    }
}
