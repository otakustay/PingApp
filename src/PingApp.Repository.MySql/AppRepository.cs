using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            throw new NotImplementedException();
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
    `LastValidUpdateNewValue`, `LanguagePriority`, `Hash`, `IsValid`
)
values (
    ?Id, ?DeveloperId, ?DeveloperName, ?DeveloperViewUrl, ?Price, ?Currency, ?Version, ?ReleaseDate,
    ?Name, ?Introduction, ?ReleaseNotes, ?PrimaryCategory, ?ViewUrl, ?IconUrl, ?FileSize,
    ?AverageUserRatingForCurrentVersion, ?UserRatingCountForCurrentVersion, ?SupportedDevices, ?Features,
    ?IsGameCenterEnabled, ?DeviceType, ?LastValidUpdateTime, ?LastValidUpdateType, ?LastValidUpdateOldValue,
    ?LastValidUpdateNewValue, ?LanguagePriority, ?Hash, ?IsValid,
);";
            MySqlCommand commandForAppBrief = connection.CreateCommand();
            commandForAppBrief.CommandText = inserAppBrief;
            commandForAppBrief.Parameters.AddWithValue("?Id", app.Brief.Id);
            commandForAppBrief.Parameters.AddWithValue("?DeveloperId", app.Brief.Developer.Id);
            commandForAppBrief.Parameters.AddWithValue("?DeveloperName", app.Brief.Developer.Name);
            commandForAppBrief.Parameters.AddWithValue("?DeveloperViewUrl", app.Brief.Developer.ViewUrl);
            commandForAppBrief.Parameters.AddWithValue("?Price", app.Brief.Price);
            commandForAppBrief.Parameters.AddWithValue("?Currency", app.Brief.Currency);
            commandForAppBrief.Parameters.AddWithValue("?Version", app.Brief.Version);
            commandForAppBrief.Parameters.AddWithValue("?ReleaseDate", app.Brief.ReleaseDate);
            commandForAppBrief.Parameters.AddWithValue("?Name", app.Brief.Name);
            commandForAppBrief.Parameters.AddWithValue("?Introduction", app.Brief.Introduction);
            commandForAppBrief.Parameters.AddWithValue("?ReleaseNotes", app.Brief.ReleaseNotes);
            commandForAppBrief.Parameters.AddWithValue("?PrimaryCategory", app.Brief.PrimaryCategory.Id);
            commandForAppBrief.Parameters.AddWithValue("?ViewUrl", app.Brief.ViewUrl);
            commandForAppBrief.Parameters.AddWithValue("?IconUrl", app.Brief.IconUrl);
            commandForAppBrief.Parameters.AddWithValue("?FileSize", app.Brief.FileSize);
            commandForAppBrief.Parameters.AddWithValue("?AverageUserRatingForCurrentVersion", app.Brief.AverageUserRatingForCurrentVersion);
            commandForAppBrief.Parameters.AddWithValue("?UserRatingCountForCurrentVersion", app.Brief.UserRatingCountForCurrentVersion);
            commandForAppBrief.Parameters.AddWithValue("?SupportedDevices", app.Brief.SupportedDevices);
            commandForAppBrief.Parameters.AddWithValue("?Features", app.Brief.Features);
            commandForAppBrief.Parameters.AddWithValue("?IsGameCenterEnabled", app.Brief.IsGameCenterEnabled);
            commandForAppBrief.Parameters.AddWithValue("?DeviceType", app.Brief.DeviceType);
            commandForAppBrief.Parameters.AddWithValue("?LastValidUpdateTime", app.Brief.LastValidUpdate.Time);
            commandForAppBrief.Parameters.AddWithValue("?LastValidUpdateType", app.Brief.LastValidUpdate.Type);
            commandForAppBrief.Parameters.AddWithValue("?LastValidUpdateOldValue", app.Brief.LastValidUpdate.OldValue);
            commandForAppBrief.Parameters.AddWithValue("?LastValidUpdateNewValue", app.Brief.LastValidUpdate.NewValue);
            commandForAppBrief.Parameters.AddWithValue("?LanguagePriority", app.Brief.LanguagePriority);
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
            commandForApp.Parameters.AddWithValue("?Id", app.Id);
            commandForApp.Parameters.AddWithValue("?CensoredName", app.CensoredName);
            commandForApp.Parameters.AddWithValue("?Description", app.Description);
            commandForApp.Parameters.AddWithValue("?LargeIconUrl", app.LargeIconUrl);
            commandForApp.Parameters.AddWithValue("?SellerName", app.Seller.Name);
            commandForApp.Parameters.AddWithValue("?SellerViewUrl", app.Seller.ViewUrl);
            commandForApp.Parameters.AddWithValue("?ReleaseNotes", app.ReleaseNotes);
            commandForApp.Parameters.AddWithValue("?ContentAdvisoryRating", app.ContentAdvisoryRating);
            commandForApp.Parameters.AddWithValue("?ContentRating", app.ContentRating);
            commandForApp.Parameters.AddWithValue("?AverageUserRating", app.AverageUserRating);
            commandForApp.Parameters.AddWithValue("?UserRatingCount", app.UserRatingCount);
            commandForApp.Parameters.AddWithValue("?Languages", String.Join(",", app.Languages));
            commandForApp.Parameters.AddWithValue("?Categories", String.Join(",", app.Categories.Select(c => c.Id)));
            commandForApp.Parameters.AddWithValue("?ScreenshotUrls", String.Join(",", app.ScreenshotUrls));
            commandForApp.Parameters.AddWithValue("?IPadScreenshotUrl", String.Join(",", app.IPadScreenshotUrls));
            commandForApp.ExecuteNonQuery();
        }

        public void Update(App app) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(int offset, int limit) {
            throw new NotImplementedException();
        }

        public RevokedApp Revoke(App app) {
            throw new NotImplementedException();
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
    }
}
