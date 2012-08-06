using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository.MySql {
    public static class Map {
        public static User ToUser(this IDataRecord record) {
            User user = new User() {
                Id = record.GetGuid("Id"), 
                Description  = record.GetString("Description"), 
                Email = record.GetString("Email"), 
                NotifyOnOwnedUpdate = record.GetBoolean("NotifyOnOwnedUpdate"), 
                NotifyOnWishFree = record.GetBoolean("NotifyOnWishFree"), 
                NotifyOnWishPriceDrop = record.GetBoolean("NotifyOnWishPriceDrop"), 
                NotifyOnWishUpdate = record.GetBoolean("NotifyOnWishUpdate"), 
                Password = record.GetString("Password"), 
                PreferredLanguagePriority = record.GetInt32("PreferredLanguagePriority"), 
                ReceiveSiteUpdates = record.GetBoolean("ReceiveSiteUpdates"), 
                RegisterTime = record.GetDateTime("RegisterTime"), 
                Status = (UserStatus)record.GetInt32("Status"), 
                Username = record.GetString("Username"), 
                Website = record.GetString("Website")
            };
            return user;
        }

        public static AppBrief ToAppBrief(this IDataRecord record, string columnPrefix = "") {
            AppBrief brief = new AppBrief() {
                AverageUserRatingForCurrentVersion = 
                    record.GetNullableFloat(columnPrefix + "AverageUserRatingForCurrentVersion"),
                Currency = record.GetString(columnPrefix + "Currency"),
                Features = record.GetStringArray(columnPrefix + "Features"),
                FileSize = record.GetInt32(columnPrefix + "FileSize"),
                IconUrl = record.GetString(columnPrefix + "IconUrl"),
                Id = record.GetInt32(columnPrefix + "Id"),
                Introduction = record.GetString(columnPrefix + "Introduction"),
                LanguagePriority = record.GetInt32(columnPrefix + "LanguagePriority"),
                Name = record.GetString(columnPrefix + "Name"),
                Price = record.GetFloat(columnPrefix + "Price"),
                PrimaryCategory = Category.Get(record.GetInt32(columnPrefix + "PrimaryCategory")),
                ReleaseDate = record.GetDateTime(columnPrefix + "ReleaseDate"),
                ReleaseNotes = record.GetString(columnPrefix + "ReleaseNotes"),
                SupportedDevices = record.GetStringArray(columnPrefix + "SupportedDevices"),
                UserRatingCountForCurrentVersion = 
                    record.GetNullableInt32(columnPrefix + "UserRatingCountForCurrentVersion"),
                Version = record.GetString(columnPrefix + "Version"),
                ViewUrl = record.GetString(columnPrefix + "ViewUrl"), 
                LastValidUpdate = new AppUpdate() {
                    NewValue = record.GetString(columnPrefix + "LastValidUpdateNewValue"),
                    OldValue = record.GetString(columnPrefix + "LastValidUpdateOldValue"),
                    Time = record.GetDateTime(columnPrefix + "LastValidUpdateTime"),
                    Type = (AppUpdateType)record.GetInt32(columnPrefix + "LastValidUpdateType")
                },
                Developer = new Developer() {
                    Id = record.GetInt32(columnPrefix + "DeveloperId"),
                    Name = record.GetString(columnPrefix + "DeveloperName"),
                    ViewUrl = record.GetString(columnPrefix + "DeveloperViewUrl")
                }
            };
            brief.LastValidUpdate.App = brief.Id;
            return brief;
        }

        public static App ToApp(this IDataRecord record, string columnPrefix = "") {
            App app = new App() {
                AverageUserRating = record.GetNullableFloat(columnPrefix + "AverageUserRating"),
                Categories = record.GetStringArray(columnPrefix + "Categories")
                    .Select(Int32.Parse).Select(i => Category.Get(i)).ToArray(),
                CensoredName = record.GetString(columnPrefix + "CensoredName"),
                ContentAdvisoryRating = record.GetString(columnPrefix + "ContentAdvisoryRating"),
                ContentRating = record.GetString(columnPrefix + "ContentRating"),
                Description = record.GetString(columnPrefix + "Description"),
                Id = record.GetInt32(columnPrefix + "Id"),
                IPadScreenshotUrls = record.GetStringArray(columnPrefix + "IPadScreenshotUrls"),
                Languages = record.GetStringArray(columnPrefix + "Languages"),
                LargeIconUrl = record.GetString(columnPrefix + "LargeIconUrl"),
                ReleaseNotes = record.GetString(columnPrefix + "ReleaseNotes"),
                ScreenshotUrls = record.GetStringArray(columnPrefix + "ScreenshotUrls"),
                UserRatingCount = record.GetNullableInt32(columnPrefix + "UserRatingCount"), 
                Seller = new Seller() {
                    Name = record.GetString(columnPrefix + "SellerName"),
                    ViewUrl = record.GetString(columnPrefix + "SellerViewUrl")
                },
                Brief = record.ToAppBrief(columnPrefix + "Brief.")
            };
            return app;
        }

        public static RevokedApp ToRevokedApp(this IDataRecord record) {
            App app = record.ToApp();
            RevokedApp revoked = new RevokedApp(app);
            revoked.RevokeTime = record.GetDateTime("RevokeTime");
            return revoked;
        }

        public static AppUpdate ToAppUpdate(this IDataRecord record) {
            AppUpdate update = new AppUpdate() {
                App = record.GetInt32("App"),
                Id = record.GetGuid("Id"), 
                NewValue = record.GetString("NewValue"), 
                OldValue = record.GetString("OldValue"), 
                Type = (AppUpdateType)record.GetInt32("Type"), 
                Time = record.GetDateTime("Time")
            };
            return update;
        }

        public static AppTrack ToAppTrack(this IDataRecord record) {
            AppTrack track = new AppTrack() {
                BuyPrice = record.GetNullableFloat("BuyPrice"), 
                BuyTime = record.GetNullableDateTime("BuyTime"), 
                CreatePrice = record.GetFloat("CreatePrice"), 
                CreateTime = record.GetDateTime("CreateTime"),
                HasRead = record.GetBoolean("HasRead"), 
                Id = record.GetGuid("Id"),
                Rate = record.GetInt32("Rate"),
                Status = (AppTrackStatus)record.GetInt32("Status"), 
                App = record.ToAppBrief("App."), 
                User = record.GetGuid("User")
            };
            return track;
        }

        #region IDataRecord 扩展方法

        public static Guid GetGuid(this IDataRecord record, string field) {
            return Guid.Parse(record[field].ToString());
        }

        public static int GetInt32(this IDataRecord record, string field) {
            return Convert.ToInt32(record[field]);
        }

        public static float GetFloat(this IDataRecord record, string field) {
            return Convert.ToSingle(record[field]);
        }

        public static bool GetBoolean(this IDataRecord record, string field) {
            return Convert.ToBoolean(record[field]);
        }

        public static DateTime GetDateTime(this IDataRecord record, string field) {
            return Convert.ToDateTime(record[field]);
        }

        public static string GetString(this IDataRecord record, string field) {
            return record[field].ToString();
        }

        public static int? GetNullableInt32(this IDataRecord record, string field) {
            return record[field] is DBNull ? (int?)null : Convert.ToInt32(record[field]);
        }

        public static float? GetNullableFloat(this IDataRecord record, string field) {
            return record[field] is DBNull ? (float?)null : Convert.ToSingle(record[field]);
        }

        public static bool? GetNullableBoolean(this IDataRecord record, string field) {
            return record[field] is DBNull ? (bool?)null : Convert.ToBoolean(record[field]);
        }

        public static DateTime? GetNullableDateTime(this IDataRecord record, string field) {
            return record[field] is DBNull ? (DateTime?)null : Convert.ToDateTime(record[field]);
        }

        public static string[] GetStringArray(this IDataRecord record, string field, char separator = ',') {
            return record[field].ToString().Split(separator);
        }

        #endregion
    }
}
