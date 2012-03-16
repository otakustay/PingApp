using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Diagnostics;
using System.IO;
using PingApp.Entity;
using MySql.Data.MySqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Data;
using PingApp.Schedule.Storage;
using PingApp.Utility;
using System.Data.Common;

namespace PingApp.Schedule.Task {
    class DbUpdateTask : TaskNode {
        private readonly DbCheckType checkType;

        private readonly bool checkOffUpdates;

        private readonly List<AppUpdate> validUpdates = new List<AppUpdate>();

        private MySqlConnection connection;

        private MySqlTransaction transaction;

        public DbUpdateTask(DbCheckType checkType, bool checkOffUpdates) {
            this.checkType = checkType;
            this.checkOffUpdates = checkOffUpdates;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start update to database using --check-type={0} --check-off-updates={1}", checkType, checkOffUpdates);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Directory.CreateDirectory(LogRoot);
            IStorage output = Action == ActionType.Initialize ?
                (IStorage)new FileSystemStorage(Path.Combine(LogRoot, "Output")) : new MemoryStorage();
            if (Action != ActionType.Initialize) {
                // UpdateWithCheck用
                output.Add("New", new List<App>());
                output.Add("Updated", new List<App>());
            }
            Buffer<App> buffer = new Buffer<App>(Program.BatchSize + 200, list => UpdateToDb(list, output));

            while (input.HasMore) {
                App[] apps = input.Get<App[]>();
                buffer.AddRange(apps);
            }
            buffer.Flush();

            // 对于增量更新的情况，再处理一下下架的应用，就是not-found.txt中的内容
            if (checkOffUpdates) {
                List<int> notFound = input.Get<List<int>>("NotFound");
                AddOffUpdates(notFound);
            }

            if (checkType == DbCheckType.CheckForUpdate) {
                Log.Info("Found {0} valid updates", validUpdates.Count);
                output.Add("Updates", validUpdates);
            }

            watch.Stop();
            Log.Info("Work done using {0:00}:{1:00}", watch.Elapsed.Minutes, watch.Elapsed.Seconds);

            return output;
        }

        private MySqlCommand CreateCommand() {
            if (connection == null) {
                return null;
            }
            return new MySqlCommand(String.Empty, connection, transaction);
        }

        private void UpdateToDb(App[] list, IStorage output) {
            using (connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                using (transaction = connection.BeginTransaction()) {
                    try {
                        // 纯写入
                        if (checkType == DbCheckType.ForceInsert) {
                            UpdateWithNoCheck(list, output);
                        }
                        // 检查更新项
                        else {
                            UpdateWithCheck(list, output);
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        Log.ErrorException("Update to db failed with --db-check-type=" + checkType, ex);
                        transaction.Rollback();
                    }
                }
            }
            connection = null;
            transaction = null;
        }

        private void UpdateWithCheck(App[] list, IStorage output) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<App> updated = new List<App>();
            List<App> added = new List<App>();

            if (checkType == DbCheckType.DiscardUpdate) {
                // 只找出需要插入的那些
                // 虽然前面RssFeed之类的已经找过，但这里有事务，更安全
                HashSet<int> exists;
                try {
                    exists = CheckApps(list.Select(a => a.Id).ToArray());
                }
                catch (Exception ex) {
                    Log.ErrorException("Error checking apps with db", ex);
                    string filename = Path.Combine(LogRoot, "error-" + list.GetHashCode() + "-" + list.Length + ".txt");
                    File.WriteAllText(filename, Utility.JsonSerialize(list), Encoding.UTF8);
                    return;
                }

                watch.Stop();
                Log.Info("{0} records checked with db using {1}ms, {2} new apps assumed", exists.Count, watch.ElapsedMilliseconds, list.Length - exists.Count);
                watch.Start();

                foreach (App app in list.Where(a => !exists.Contains(a.Id))) {
                    InsertNewApp(app);
                    added.Add(app);
                }
            }
            else {
                // 全量更新
                Dictionary<int, FullApp> compareBase;
                try {
                    compareBase = GetApps(list.Select(a => a.Id).ToArray());
                }
                catch (Exception ex) {
                    Log.ErrorException("Error retrieving apps from db", ex);
                    string filename = Path.Combine(LogRoot, "error-" + list.GetHashCode() + "-" + list.Length + ".txt");
                    File.WriteAllText(filename, Utility.JsonSerialize(list), Encoding.UTF8);
                    return;
                }

                watch.Stop();
                Log.Info("{0} records retrieved from db using {1}ms, {2} new apps assumed", compareBase.Count, watch.ElapsedMilliseconds, list.Length - compareBase.Count);
                watch.Start();

                foreach (App app in list) {
                    // 更新
                    if (compareBase.ContainsKey(app.Id)) {
                        FullApp origin = compareBase[app.Id];
                        if (!origin.Equals(app)) {
                            try {
                                UpdateApp(origin, app);
                                updated.Add(app);
                            }
                            catch (DbException ex) {
                                Log.ErrorException("Failed update app " + app.Id + " manually change hash to " + Utility.ComputeAppHash(app, 0), ex);
                            }
                        }
                    }
                    // 新建，理论上不会有这一环节
                    else {
                        Log.Warn("Weired hit at insert branch on --check-type=CheckForUpdate");
                        InsertNewApp(app);
                        added.Add(app);
                    }
                }
            }

            output.Get<List<App>>("New").AddRange(added);
            output.Get<List<App>>("Updated").AddRange(updated);

            Log.Info("{0} records commited using {1}ms", added.Count + updated.Count, watch.ElapsedMilliseconds);
            Log.Debug("Added: " + String.Join(",", added.Select(a => a.Id).ToArray()));
            Log.Debug("Updated: " + String.Join(",", updated.Select(a => a.Id).ToArray()));
        }

        private void UpdateWithNoCheck(App[] list, IStorage output) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            try {
                foreach (App app in list) {
                    InsertNewApp(app);
                }
                watch.Stop();
                Log.Info("{0} records commited using {1}ms", list.Length, watch.ElapsedMilliseconds);
            }
            catch (Exception ex) {
                Log.ErrorException("InsertWithNoCheck failed", ex);
                File.WriteAllText(
                    Path.Combine(LogRoot, "error-" + list.GetHashCode() + "-" + list.Length + ".txt"),
                    Utility.JsonSerialize(list),
                    Encoding.UTF8
                );
            }

            if (Action == ActionType.Initialize) {
                output.Add(list);
            }
        }

        private void InsertNewApp(App app) {
            app.Brief.LastValidUpdate = new AppUpdate() {
                Time = DateTime.Now,
                Type = AppUpdateType.New,
                NewValue = String.Empty,
                OldValue = String.Empty
            };
            InsertApp(app);
            AppUpdate updateForNew = new AppUpdate() {
                App = app.Id,
                Time = app.Brief.ReleaseDate.Date,
                Type = AppUpdateType.New,
                OldValue = app.Brief.Version + ", $" + app.Brief.Price
            };
            AddAppUpdate(updateForNew);
            AppUpdate updateForAdd = new AppUpdate() {
                App = app.Id,
                Time = app.Brief.LastValidUpdate.Time,
                Type = AppUpdateType.AddToNote,
                OldValue = app.Brief.Version + ", $" + app.Brief.Price
            };
            AddAppUpdate(updateForAdd);
        }

        private void InsertApp(App app) {
            MySqlCommand cmdForApp = CreateCommand();
            cmdForApp.CommandText =
@"insert into PingApp.App (
    `Id`, `Description`, `LargeIconUrl`, `SellerName`, `ReleaseNotes`, `ContentAdvisoryRating`, 
    `CensoredName`, `ScreenshotUrls`, `IPadScreenshotUrls`, `SellerViewUrl`, 
    `ContentRating`, `AverageUserRating`, `UserRatingCount`, `Languages`, `Categories`
)
values (
    ?Id, ?Description, ?LargeIconUrl, ?SellerName, ?ReleaseNotes, ?ContentAdvisoryRating, 
    ?CensoredName, ?ScreenshotUrls, ?IPadScreenshotUrls, ?SellerViewUrl, 
    ?ContentRating, ?AverageUserRating, ?UserRatingCount, ?Languages, ?Categories
);";
            cmdForApp.Parameters.AddWithValue("?Id", app.Id);
            cmdForApp.Parameters.AddWithValue("?Description", app.Description);
            cmdForApp.Parameters.AddWithValue("?LargeIconUrl", app.LargeIconUrl);
            cmdForApp.Parameters.AddWithValue("?SellerName", app.Seller.Name ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?ReleaseNotes", app.ReleaseNotes ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?ContentAdvisoryRating", app.ContentAdvisoryRating ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?CensoredName", app.CensoredName);
            cmdForApp.Parameters.AddWithValue("?ScreenshotUrls", String.Join(",", app.ScreenshotUrls));
            cmdForApp.Parameters.AddWithValue("?IPadScreenshotUrls", String.Join(",", app.IPadScreenshotUrls));
            cmdForApp.Parameters.AddWithValue("?SellerViewUrl", app.Seller.ViewUrl ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?ContentRating", app.ContentRating ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?AverageUserRating", app.AverageUserRating);
            cmdForApp.Parameters.AddWithValue("?UserRatingCount", app.UserRatingCount);
            cmdForApp.Parameters.AddWithValue("?Languages", String.Join(",", app.Languages));
            cmdForApp.Parameters.AddWithValue("?Categories", String.Join(",", app.Categories.Select(c => c.Id).ToArray()));
            cmdForApp.ExecuteNonQuery();

            MySqlCommand cmdForBrief = CreateCommand();
            cmdForBrief.CommandText =
@"insert into PingApp.AppBrief (
    `Id`, `DeveloperId`, `DeveloperName`, `Price`, `Version`, `ReleaseDate`, `Currency`, `Name`, 
    `Introduction`, `ReleaseNotes`, `PrimaryCategory`, `DeveloperViewUrl`, `ViewUrl`,
    `IconUrl`, `FileSize`, `AverageUserRatingForCurrentVersion`, `UserRatingCountForCurrentVersion`, 
    `SupportedDevices`, `Features`, `IsGameCenterEnabled`, `DeviceType`, `LanguagePriority`,
    `LastValidUpdateTime`, `LastValidUpdateType`, `LastValidUpdateOldValue`, `LastValidUpdateNewValue`, `Hash`, `IsValid`
)
values (
    ?Id, ?DeveloperId, ?DeveloperName, ?Price, ?Version, ?ReleaseDate, ?Currency, ?Name, 
    ?Introduction, ?ReleaseNotes, ?PrimaryCategory, ?DeveloperViewUrl, ?ViewUrl, 
    ?IconUrl, ?FileSize, ?AverageUserRatingForCurrentVersion, ?UserRatingCountForCurrentVersion, 
    ?SupportedDevices, ?Features, ?IsGameCenterEnabled, ?DeviceType, ?LanguagePriority,
    ?LastValidUpdateTime, ?LastValidUpdateType, ?LastValidUpdateOldValue, ?LastValidUpdateNewValue, ?Hash, 1
);";
            cmdForBrief.Parameters.AddWithValue("?Id", app.Id);
            cmdForBrief.Parameters.AddWithValue("?DeveloperId", app.Brief.Developer.Id);
            cmdForBrief.Parameters.AddWithValue("?DeveloperName", app.Brief.Developer.Name ?? String.Empty);
            cmdForBrief.Parameters.AddWithValue("?Price", app.Brief.Price);
            cmdForBrief.Parameters.AddWithValue("?Version", app.Brief.Version);
            cmdForBrief.Parameters.AddWithValue("?ReleaseDate", app.Brief.ReleaseDate);
            cmdForBrief.Parameters.AddWithValue("?Currency", app.Brief.Currency);
            cmdForBrief.Parameters.AddWithValue("?Name", app.Brief.Name);
            cmdForBrief.Parameters.AddWithValue("?Introduction", app.Brief.Introduction);
            cmdForBrief.Parameters.AddWithValue("?ReleaseNotes", app.Brief.ReleaseNotes);
            cmdForBrief.Parameters.AddWithValue("?PrimaryCategory", app.Brief.PrimaryCategory.Id);
            cmdForBrief.Parameters.AddWithValue("?DeveloperViewUrl", app.Brief.Developer.ViewUrl ?? String.Empty);
            cmdForBrief.Parameters.AddWithValue("?ViewUrl", app.Brief.ViewUrl);
            cmdForBrief.Parameters.AddWithValue("?IconUrl", app.Brief.IconUrl);
            cmdForBrief.Parameters.AddWithValue("?FileSize", app.Brief.FileSize);
            cmdForBrief.Parameters.AddWithValue("?AverageUserRatingForCurrentVersion", app.Brief.AverageUserRatingForCurrentVersion);
            cmdForBrief.Parameters.AddWithValue("?UserRatingCountForCurrentVersion", app.Brief.UserRatingCountForCurrentVersion);
            cmdForBrief.Parameters.AddWithValue("?SupportedDevices", String.Join(",", app.Brief.SupportedDevices.ToArray()));
            cmdForBrief.Parameters.AddWithValue("?Features", String.Join(",", app.Brief.Features.ToArray()));
            cmdForBrief.Parameters.AddWithValue("?IsGameCenterEnabled", app.Brief.IsGameCenterEnabled);
            cmdForBrief.Parameters.AddWithValue("?DeviceType", app.Brief.DeviceType);
            cmdForBrief.Parameters.AddWithValue("?LanguagePriority", app.Brief.LanguagePriority);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateTime", app.Brief.LastValidUpdate.Time);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateType", app.Brief.LastValidUpdate.Type);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateOldValue", app.Brief.LastValidUpdate.OldValue);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateNewValue", app.Brief.LastValidUpdate.NewValue);
            cmdForBrief.Parameters.AddWithValue("?Hash", "00" + Utility.ComputeAppHash(app, 0));
            cmdForBrief.ExecuteNonQuery();

        }

        private void AddAppUpdate(AppUpdate update) {
            MySqlCommand cmd = CreateCommand();
            cmd.CommandText =
@"insert into PingApp.AppUpdate (
    `App`, `Time`, `Type`, `OldValue`, `NewValue`
) values (
    ?App, ?Time, ?Type, ?OldValue, ?NewValue
)";
            cmd.Parameters.AddWithValue("?App", update.App);
            cmd.Parameters.AddWithValue("?Time", update.Time);
            cmd.Parameters.AddWithValue("?Type", update.Type);
            cmd.Parameters.AddWithValue("?OldValue", update.OldValue ?? String.Empty);
            cmd.Parameters.AddWithValue("?NewValue", update.NewValue ?? String.Empty);
            cmd.ExecuteNonQuery();
        }

        private void UpdateApp(FullApp origin, App app) {
            List<AppUpdate> updates = origin.Brief.CheckForUpdate(app.Brief);
            validUpdates.AddRange(updates.Where(u => AppUpdate.IsValidUpdate(u.Type)));
            // 添加更新信息
            foreach (AppUpdate update in updates) {
                AddAppUpdate(update);
                if (AppUpdate.IsValidUpdate(update.Type)) {
                    app.Brief.LastValidUpdate = update;
                }
            }
            if (app.Brief.LastValidUpdate == null) {
                app.Brief.LastValidUpdate = origin.Brief.LastValidUpdate;
            }
            // 更新全数据
            MySqlCommand cmdForApp = CreateCommand();
            cmdForApp.CommandText =
@"update PingApp.App
set
    `Description` = ?Description, `LargeIconUrl` = ?LargeIconUrl, `SellerName` = ?SellerName, `ReleaseNotes` = ?ReleaseNotes, 
    `ContentAdvisoryRating` = ?ContentAdvisoryRating, `CensoredName` = ?CensoredName, `ScreenshotUrls` = ?ScreenshotUrls, 
    `IPadScreenshotUrls` = ?IPadScreenshotUrls, `SellerViewUrl` = ?SellerViewUrl, `ContentRating` = ?ContentRating, 
    `AverageUserRating` = ?AverageUserRating, `UserRatingCount` = ?UserRatingCount, `Languages` = ?Languages, `Categories` = ?Categories 
where
    `Id` = ?Id";
            cmdForApp.Parameters.AddWithValue("?Description", app.Description ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?LargeIconUrl", app.LargeIconUrl);
            cmdForApp.Parameters.AddWithValue("?SellerName", app.Seller.Name ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?ReleaseNotes", app.ReleaseNotes ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?ContentAdvisoryRating", app.ContentAdvisoryRating ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?CensoredName", app.CensoredName);
            cmdForApp.Parameters.AddWithValue("?ScreenshotUrls", String.Join(",", app.ScreenshotUrls));
            cmdForApp.Parameters.AddWithValue("?IPadScreenshotUrls", String.Join(",", app.IPadScreenshotUrls));
            cmdForApp.Parameters.AddWithValue("?SellerViewUrl", app.Seller.ViewUrl ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?ContentRating", app.ContentRating ?? String.Empty);
            cmdForApp.Parameters.AddWithValue("?AverageUserRating", app.AverageUserRating);
            cmdForApp.Parameters.AddWithValue("?UserRatingCount", app.UserRatingCount);
            cmdForApp.Parameters.AddWithValue("?Languages", String.Join(",", app.Languages));
            cmdForApp.Parameters.AddWithValue("?Categories", String.Join(",", app.Categories.Select(c => c.Id).ToArray()));
            cmdForApp.Parameters.AddWithValue("?Id", app.Id);
            cmdForApp.ExecuteNonQuery();

            MySqlCommand cmdForBrief = CreateCommand();
            cmdForBrief.CommandText =
@"update PingApp.AppBrief
set
    `DeveloperId` = ?DeveloperId, `DeveloperName` = ?DeveloperName, `Price` = ?Price, `Version` = ?Version,
    `ReleaseDate` = ?ReleaseDate, `Currency` = ?Currency, `Name` = ?Name, `Introduction` = ?Introduction, 
    `ReleaseNotes` = ?ReleaseNotes, `PrimaryCategory` = ?PrimaryCategory, 
    `DeveloperViewUrl` = ?DeveloperViewUrl, `ViewUrl` = ?ViewUrl, `IconUrl` = ?IconUrl, `FileSize` = ?FileSize, 
    `AverageUserRatingForCurrentVersion` = ?AverageUserRatingForCurrentVersion, 
    `UserRatingCountForCurrentVersion` = ?UserRatingCountForCurrentVersion, `SupportedDevices` = ?SupportedDevices, 
    `IsGameCenterEnabled` = ?IsGameCenterEnabled, `DeviceType` = ?DeviceType, `Features` = ?Features, 
    `LastValidUpdateTime` = ?LastValidUpdateTime, `LastValidUpdateType` = ?LastValidUpdateType, `LanguagePriority` = ?LanguagePriority,
    `LastValidUpdateOldValue` = ?LastValidUpdateOldValue, `LastValidUpdateNewValue` = ?LastValidUpdateNewValue, `Hash` = ?Hash, `IsValid` = 1
where
    `Id` = ?Id";
            cmdForBrief.Parameters.AddWithValue("?DeveloperId", app.Brief.Developer.Id);
            cmdForBrief.Parameters.AddWithValue("?DeveloperName", app.Brief.Developer.Name ?? String.Empty);
            cmdForBrief.Parameters.AddWithValue("?Price", app.Brief.Price);
            cmdForBrief.Parameters.AddWithValue("?Version", app.Brief.Version);
            cmdForBrief.Parameters.AddWithValue("?ReleaseDate", app.Brief.ReleaseDate);
            cmdForBrief.Parameters.AddWithValue("?Currency", app.Brief.Currency);
            cmdForBrief.Parameters.AddWithValue("?Name", app.Brief.Name);
            cmdForBrief.Parameters.AddWithValue("?Introduction", app.Brief.Introduction ?? String.Empty);
            cmdForBrief.Parameters.AddWithValue("?ReleaseNotes", app.Brief.ReleaseNotes);
            cmdForBrief.Parameters.AddWithValue("?PrimaryCategory", app.Brief.PrimaryCategory.Id);
            cmdForBrief.Parameters.AddWithValue("?DeveloperViewUrl", app.Brief.Developer.ViewUrl ?? String.Empty);
            cmdForBrief.Parameters.AddWithValue("?ViewUrl", app.Brief.ViewUrl);
            cmdForBrief.Parameters.AddWithValue("?IconUrl", app.Brief.IconUrl);
            cmdForBrief.Parameters.AddWithValue("?FileSize", app.Brief.FileSize);
            cmdForBrief.Parameters.AddWithValue("?AverageUserRatingForCurrentVersion", app.Brief.AverageUserRatingForCurrentVersion);
            cmdForBrief.Parameters.AddWithValue("?UserRatingCountForCurrentVersion", app.Brief.UserRatingCountForCurrentVersion);
            cmdForBrief.Parameters.AddWithValue("?SupportedDevices", String.Join(",", app.Brief.SupportedDevices.ToArray()));
            cmdForBrief.Parameters.AddWithValue("?Features", String.Join(",", app.Brief.Features.ToArray()));
            cmdForBrief.Parameters.AddWithValue("?IsGameCenterEnabled", app.Brief.IsGameCenterEnabled);
            cmdForBrief.Parameters.AddWithValue("?DeviceType", app.Brief.DeviceType);
            cmdForBrief.Parameters.AddWithValue("?LanguagePriority", app.Brief.LanguagePriority);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateTime", app.Brief.LastValidUpdate.Time);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateType", app.Brief.LastValidUpdate.Type);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateOldValue", app.Brief.LastValidUpdate.OldValue);
            cmdForBrief.Parameters.AddWithValue("?LastValidUpdateNewValue", app.Brief.LastValidUpdate.NewValue);
            cmdForBrief.Parameters.AddWithValue("?Hash", origin.Hash.Substring(0, 2) + Utility.ComputeAppHash(app, origin.Changeset));
            cmdForBrief.Parameters.AddWithValue("?Id", app.Id);
            cmdForBrief.ExecuteNonQuery();

            // 更新Track数据
            MySqlCommand cmdForTrack = CreateCommand();
            cmdForTrack.CommandText = "update AppTrack set HasRead = 0 where App = ?App";
            cmdForTrack.Parameters.AddWithValue("?App", app.Id);
            int rows = cmdForTrack.ExecuteNonQuery();
            Log.Debug("{0} track infos updated for app {1} - {2}", rows, app.Id, app.Brief.Name);
        }

        private Dictionary<int, FullApp> GetApps(int[] list) {
            Dictionary<int, FullApp> apps = new Dictionary<int, FullApp>(list.Length);
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
    b.LastValidUpdateOldValue `b.LastValidUpdateOldValue`, b.LastValidUpdateNewValue `b.LastValidUpdateNewValue`, 
    b.LanguagePriority `b.LanguagePriority`, b.Hash `b.Hash`
from PingApp.AppBrief b
inner join PingApp.App a on a.Id = b.Id
where b.Id in ({0})";

            MySqlCommand cmd = CreateCommand();
            cmd.CommandText = String.Format(sql, String.Join(",", list));
            using (IDataReader reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    FullApp app = new FullApp() {
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
                        },
                        Hash = reader.Get<string>("b.Hash")
                    };
                    apps[app.Id] = app;
                }
            }

            return apps;
        }

        private HashSet<int> CheckApps(int[] list) {
            HashSet<int> apps = new HashSet<int>();
            MySqlCommand cmd = CreateCommand();
            cmd.CommandText = String.Format("select Id from AppBrief where Id in ({0})", String.Join(",", list));
            using (IDataReader reader = cmd.ExecuteReader()) {
                cmd.CommandTimeout = 0;
                while (reader.Read()) {
                    apps.Add(reader.GetInt32(0));
                }
            }
            return apps;
        }

        private void AddOffUpdates(List<int> list) {
            Log.Info("Start add off updates");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            DateTime now = DateTime.Now;
            DateTime today = DateTime.Today;
            int count = 0;

            using (connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                using (transaction = connection.BeginTransaction()) {
                    try {
                        foreach (int id in list) {
                            // 先确定是不是已经Off了
                            MySqlCommand cmd = CreateCommand();
                            cmd.CommandText =
                                "select IsValid, Price, Version from PingApp.AppBrief where Id = ?Id";
                            cmd.Parameters.AddWithValue("?Id", id);
                            Tuple<bool, float, string> fromDb;
                            using (IDataReader reader = cmd.ExecuteReader()) {
                                reader.Read();
                                fromDb = new Tuple<bool, float, string>(
                                    reader.GetBoolean(0),
                                    reader.GetFloat(1),
                                    reader.GetString(2)
                                );
                            }
                            if (fromDb.Item1) {
                                AppUpdate update = new AppUpdate() {
                                    App = id,
                                    Time = now,
                                    Type = AppUpdateType.Off,
                                    OldValue = fromDb.Item3 + ", $" + fromDb.Item2
                                };
                                AddAppUpdate(update);
                                count++;

                                // 更新为IsValid为false
                                cmd = CreateCommand();
                                cmd.CommandText = "update PingApp.AppBrief set IsValid = 0 where Id = ?Id";
                                cmd.Parameters.AddWithValue("?Id", id);
                                cmd.ExecuteNonQuery();
                            }
                            else {
                                Log.Trace("Off update for {0} is dismissed because the app is already invalid", id);
                            }
                            // Off不作为ValidUpdate更新
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.ErrorException("Add off updates failed", ex);
                    }
                }
            }

            connection = null;
            transaction = null;

            watch.Stop();
            Log.Info("{0} off updates added using {1}ms, {2} dismissed", count, watch.ElapsedMilliseconds, list.Count - count);
        }
    }
}
