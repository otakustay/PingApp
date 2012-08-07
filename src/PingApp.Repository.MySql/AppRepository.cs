using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.MySql {
    public class AppRepository : IAppRepository, IDisposable {
        private readonly MySqlConnection connection;

        public AppRepository(MySqlConnection connection) {
            this.connection = connection;
        }

        public App Retrieve(int app) {
            string sql = "select * from AppWithBrief where Id = ?Id;";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?Id", app);
            using (IDataReader reader = command.ExecuteReader()) {
                if (reader.Read()) {
                    App result = reader.ToApp();
                    return result;
                }
            }
            return null;
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            string sql = String.Format("select * from AppWithBrief where Id in ({0});", String.Join(",", required));
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            List<App> result = new List<App>();
            using (IDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    App app = reader.ToApp();
                    result.Add(app);
                }
            }
            return result;
        }

        public ICollection<AppBrief> RetrieveBriefs(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public DeveloperAppsQuery RetrieveByDeveloper(DeveloperAppsQuery query) {
            throw new NotImplementedException();
        }

        public AppListQuery Search(Quries.AppListQuery query) {
            throw new NotImplementedException();
        }

        public void Save(App app) {
            string inserAppBrief =
@"insert into `AppBrief` (
    `Id`, `DeveloperId`, `DeveloperName`, `DeveloperViewUrl`, `Price`, `Currency`, `Version`, `ReleaseDate`, 
    `Name`, `Introduction`, `ReleaseNotes`, `PrimaryCategory`, `ViewUrl`, `IconUrl`, `FileSize`, 
    `AverageUserRatingForCurrentVersion`, `UserRatingCountForCurrentVersion`, `SupportedDevices`, `Features`,
    `IsGameCenterEnabled`, `DeviceType`, `LastValidUpdateTime`, `LastValidUpdateType`, `LastValidUpdateOldValue`,
    `LastValidUpdateNewValue`, `LanguagePriority`
)
values (
    ?Id, ?DeveloperId, ?DeveloperName, ?DeveloperViewUrl, ?Price, ?Currency, ?Version, ?ReleaseDate,
    ?Name, ?Introduction, ?ReleaseNotes, ?PrimaryCategory, ?ViewUrl, ?IconUrl, ?FileSize,
    ?AverageUserRatingForCurrentVersion, ?UserRatingCountForCurrentVersion, ?SupportedDevices, ?Features,
    ?IsGameCenterEnabled, ?DeviceType, ?LastValidUpdateTime, ?LastValidUpdateType, ?LastValidUpdateOldValue,
    ?LastValidUpdateNewValue, ?LanguagePriority
);";
            MySqlCommand commandForAppBrief = connection.CreateCommand();
            commandForAppBrief.CommandText = inserAppBrief;
            AddParametersForApp(app, commandForAppBrief);
            commandForAppBrief.ExecuteNonQuery();

            string insertApp =
@"insert into `App` (
    `Id`, `CensoredName`, `Description`, `LargeIconUrl`, `SellerName`, `SellerViewUrl`, 
    `ReleaseNotes`, `ContentAdvisoryRating`, `ContentRating`, `AverageUserRating`, 
    `UserRatingCount`, `Languages`, `Categories`, `ScreenshotUrls`, `IPadScreenshotUrls`
)
values (
    ?Id, ?CensoredName, ?Description, ?LargeIconUrl, ?SellerName, ?SellerViewUrl,
    ?ReleaseNotes, ?ContentAdvisoryRating, ?ContentRating, ?AverageUserRating, 
    ?UserRatingCount, ?Languages, ?Categories, ?ScreenshotUrls, ?IPadScreenshotUrl,
);";
            MySqlCommand commandForApp = connection.CreateCommand();
            commandForApp.CommandText = insertApp;
            AddParametersForAppBrief(app, commandForApp);
            commandForApp.ExecuteNonQuery();
        }

        public void Update(App app) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(int offset, int limit) {
            string sql = "select Id from AppBrief limit ?offset, ?limit";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?offset", offset);
            command.Parameters.AddWithValue("?limit", limit);
            List<int> page = new List<int>();
            using (IDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    page.Add(reader.GetInt32(0));
                }
            }

            return Retrieve(page);
        }

        public RevokedApp Revoke(App app) {
            // 从在售应用中移除
            string deleteApp = "delete from App where Id = ?Id";
            MySqlCommand commandForDeleteApp = connection.CreateCommand();
            commandForDeleteApp.CommandText = deleteApp;
            commandForDeleteApp.Parameters.AddWithValue("?Id", app.Id);
            commandForDeleteApp.ExecuteNonQuery();

            string deleteAppBrief = "delete from AppBrief where Id = ?Id";
            MySqlCommand commandForDeleteAppBrief = connection.CreateCommand();
            commandForDeleteAppBrief.CommandText = deleteAppBrief;
            commandForDeleteAppBrief.Parameters.AddWithValue("?Id", app.Id);
            commandForDeleteAppBrief.ExecuteNonQuery();

            // 添加到下架应用中
            RevokedApp revoked = new RevokedApp(app);
            revoked.RevokeTime = DateTime.Now;
            string inserRevokedAppBrief =
@"insert into `RevokedAppBrief` (
    `Id`, `DeveloperId`, `DeveloperName`, `DeveloperViewUrl`, `Price`, `Currency`, `Version`, `ReleaseDate`, 
    `Name`, `Introduction`, `ReleaseNotes`, `PrimaryCategory`, `ViewUrl`, `IconUrl`, `FileSize`, 
    `AverageUserRatingForCurrentVersion`, `UserRatingCountForCurrentVersion`, `SupportedDevices`, `Features`,
    `IsGameCenterEnabled`, `DeviceType`, `LastValidUpdateTime`, `LastValidUpdateType`, `LastValidUpdateOldValue`,
    `LastValidUpdateNewValue`, `LanguagePriority`, `RevokedTime`
)
values (
    ?Id, ?DeveloperId, ?DeveloperName, ?DeveloperViewUrl, ?Price, ?Currency, ?Version, ?ReleaseDate,
    ?Name, ?Introduction, ?ReleaseNotes, ?PrimaryCategory, ?ViewUrl, ?IconUrl, ?FileSize,
    ?AverageUserRatingForCurrentVersion, ?UserRatingCountForCurrentVersion, ?SupportedDevices, ?Features,
    ?IsGameCenterEnabled, ?DeviceType, ?LastValidUpdateTime, ?LastValidUpdateType, ?LastValidUpdateOldValue,
    ?LastValidUpdateNewValue, ?LanguagePriority, ?RevokeTime
);";
            MySqlCommand commandForRevokedAppBrief = connection.CreateCommand();
            commandForRevokedAppBrief.CommandText = inserRevokedAppBrief;
            AddParametersForApp(app, commandForRevokedAppBrief);
            commandForRevokedAppBrief.Parameters.AddWithValue("?RevokeTime", revoked.RevokeTime);
            commandForRevokedAppBrief.ExecuteNonQuery();

            string insertApp =
@"insert into `RevokedApp` (
    `Id`, `CensoredName`, `Description`, `LargeIconUrl`, `SellerName`, `SellerViewUrl`, 
    `ReleaseNotes`, `ContentAdvisoryRating`, `ContentRating`, `AverageUserRating`, 
    `UserRatingCount`, `Languages`, `Categories`, `ScreenshotUrls`, `IPadScreenshotUrls`
)
values (
    ?Id, ?CensoredName, ?Description, ?LargeIconUrl, ?SellerName, ?SellerViewUrl,
    ?ReleaseNotes, ?ContentAdvisoryRating, ?ContentRating, ?AverageUserRating, 
    ?UserRatingCount, ?Languages, ?Categories, ?ScreenshotUrls, ?IPadScreenshotUrl,
);";
            MySqlCommand commandForRevokedApp = connection.CreateCommand();
            commandForRevokedApp.CommandText = insertApp;
            AddParametersForAppBrief(app, commandForRevokedApp);
            commandForRevokedApp.ExecuteNonQuery();

            return revoked;
        }

        public ICollection<RevokedApp> RetrieveRevoked(int offset, int limit) {
            throw new NotImplementedException();
        }

        public void Resurrect(App resurrected) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            connection.Dispose();
        }

        private static void AddParametersForAppBrief(App app, MySqlCommand command) {
            command.Parameters.AddWithValue("?Id", app.Id);
            command.Parameters.AddWithValue("?CensoredName", app.CensoredName);
            command.Parameters.AddWithValue("?Description", app.Description);
            command.Parameters.AddWithValue("?LargeIconUrl", app.LargeIconUrl);
            command.Parameters.AddWithValue("?SellerName", app.Seller.Name);
            command.Parameters.AddWithValue("?SellerViewUrl", app.Seller.ViewUrl);
            command.Parameters.AddWithValue("?ReleaseNotes", app.ReleaseNotes);
            command.Parameters.AddWithValue("?ContentAdvisoryRating", app.ContentAdvisoryRating);
            command.Parameters.AddWithValue("?ContentRating", app.ContentRating);
            command.Parameters.AddWithValue("?AverageUserRating", app.AverageUserRating);
            command.Parameters.AddWithValue("?UserRatingCount", app.UserRatingCount);
            command.Parameters.AddWithValue("?Languages", String.Join(",", app.Languages));
            command.Parameters.AddWithValue("?Categories", String.Join(",", app.Categories.Select(c => c.Id)));
            command.Parameters.AddWithValue("?ScreenshotUrls", String.Join(",", app.ScreenshotUrls));
            command.Parameters.AddWithValue("?IPadScreenshotUrl", String.Join(",", app.IPadScreenshotUrls));
        }

        private static void AddParametersForApp(App app, MySqlCommand command) {
            command.Parameters.AddWithValue("?Id", app.Brief.Id);
            command.Parameters.AddWithValue("?DeveloperId", app.Brief.Developer.Id);
            command.Parameters.AddWithValue("?DeveloperName", app.Brief.Developer.Name);
            command.Parameters.AddWithValue("?DeveloperViewUrl", app.Brief.Developer.ViewUrl);
            command.Parameters.AddWithValue("?Price", app.Brief.Price);
            command.Parameters.AddWithValue("?Currency", app.Brief.Currency);
            command.Parameters.AddWithValue("?Version", app.Brief.Version);
            command.Parameters.AddWithValue("?ReleaseDate", app.Brief.ReleaseDate);
            command.Parameters.AddWithValue("?Name", app.Brief.Name);
            command.Parameters.AddWithValue("?Introduction", app.Brief.Introduction);
            command.Parameters.AddWithValue("?ReleaseNotes", app.Brief.ReleaseNotes);
            command.Parameters.AddWithValue("?PrimaryCategory", app.Brief.PrimaryCategory.Id);
            command.Parameters.AddWithValue("?ViewUrl", app.Brief.ViewUrl);
            command.Parameters.AddWithValue("?IconUrl", app.Brief.IconUrl);
            command.Parameters.AddWithValue("?FileSize", app.Brief.FileSize);
            command.Parameters.AddWithValue("?AverageUserRatingForCurrentVersion", app.Brief.AverageUserRatingForCurrentVersion);
            command.Parameters.AddWithValue("?UserRatingCountForCurrentVersion", app.Brief.UserRatingCountForCurrentVersion);
            command.Parameters.AddWithValue("?SupportedDevices", app.Brief.SupportedDevices);
            command.Parameters.AddWithValue("?Features", app.Brief.Features);
            command.Parameters.AddWithValue("?IsGameCenterEnabled", app.Brief.IsGameCenterEnabled);
            command.Parameters.AddWithValue("?DeviceType", app.Brief.DeviceType);
            command.Parameters.AddWithValue("?LastValidUpdateTime", app.Brief.LastValidUpdate.Time);
            command.Parameters.AddWithValue("?LastValidUpdateType", app.Brief.LastValidUpdate.Type);
            command.Parameters.AddWithValue("?LastValidUpdateOldValue", app.Brief.LastValidUpdate.OldValue);
            command.Parameters.AddWithValue("?LastValidUpdateNewValue", app.Brief.LastValidUpdate.NewValue);
            command.Parameters.AddWithValue("?LanguagePriority", app.Brief.LanguagePriority);
        }
    }
}
