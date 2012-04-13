using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class AppBrief : IUpdateTarget {
        private static readonly Dictionary<string, string> currencySymbolMapping = new Dictionary<string, string>() {
            { "USD", "$" },
            { "CNY", "￥" }
        };

        public int Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public DateTime ReleaseDate { get; set; }

        public float Price { get; set; }

        public string Currency { get; set; }

        public int FileSize { get; set; }

        public string ViewUrl { get; set; }

        public string IconUrl { get; set; }

        public string Introduction { get; set; }

        public string ReleaseNotes { get; set; }

        public Category PrimaryCategory { get; set; }

        public Developer Developer { get; set; }

        public float? AverageUserRatingForCurrentVersion { get; set; }

        public int? UserRatingCountForCurrentVersion { get; set; }

        public string[] Features { get; set; }

        public string[] SupportedDevices { get; set; }

        public AppUpdate LastValidUpdate { get; set; }

        public int LanguagePriority { get; set; }

        public string Hash { get; set; }

        public ICollection<AppUpdate> CheckForUpdate(AppBrief newOne) {
            DateTime now = DateTime.Now;
            List<AppUpdate> updates = new List<AppUpdate>();
            // 检查版本
            if (Version != newOne.Version) {
                AppUpdate update = new AppUpdate() {
                    App = Id,
                    Time = now,
                    NewValue = newOne.Version,
                    OldValue = Version,
                    Type = AppUpdateType.NewRelease,
                };
                updates.Add(update);
            }
            // 检查价格，价格单位有变动时涉及换算问题，不计入价格变化中
            if (Price != newOne.Price && Currency == newOne.Currency) {
                AppUpdate update = new AppUpdate() {
                    App = Id,
                    Time = now,
                    NewValue = newOne.Price.ToString(),
                    OldValue = Price.ToString(),
                    Type = newOne.Price == 0 ? AppUpdateType.PriceFree :
                        (newOne.Price > Price ? AppUpdateType.PriceIncrease : AppUpdateType.PriceDecrease)
                };
                updates.Add(update);
            }

            return updates;
        }

        public AppBrief() {
            Name = String.Empty;
            Version = String.Empty;
            Currency = "USD";
            ViewUrl = String.Empty;
            IconUrl = String.Empty;
            Introduction = String.Empty;
            ReleaseNotes = String.Empty;
        }

        public override bool Equals(object obj) {
            AppBrief other = obj as AppBrief;
            if (other == null) {
                return false;
            }

            // 不需要计算LastUpdate
            EqualsBuilder builder = new EqualsBuilder();
            builder.Append(Id, other.Id);
            builder.Append(Name, other.Name);
            builder.Append(Version, other.Version);
            builder.Append(ReleaseDate, other.ReleaseDate);
            builder.Append(Price, other.Price);
            builder.Append(Currency, other.Currency);
            builder.Append(FileSize, other.FileSize);
            builder.Append(ViewUrl, other.ViewUrl);
            builder.Append(PrimaryCategory, other.PrimaryCategory);
            builder.Append(Developer, other.Developer);
            builder.Append(AverageUserRatingForCurrentVersion, other.AverageUserRatingForCurrentVersion);
            builder.Append(UserRatingCountForCurrentVersion, other.UserRatingCountForCurrentVersion);
            builder.Append(DeviceType, other.DeviceType);
            builder.Append(LanguagePriority, other.LanguagePriority);
            builder.AppendSequence(Features, other.Features);
            builder.AppendSequence(SupportedDevices, other.SupportedDevices);
            return builder.AreEqual;
        }

        public override int GetHashCode() {
            return Id;
        }

        public DeviceType DeviceType {
            get {
                if (Features == null || SupportedDevices == null) {
                    return Entity.DeviceType.NotProvided;
                }

                bool universal = Features.Contains("iosUniversal");
                bool all = SupportedDevices.Contains("all");
                bool iPhone = SupportedDevices.Any(d => d.StartsWith("iPhone"));
                bool iPad = SupportedDevices.Any(d => d.StartsWith("iPad"));
                if (universal) {
                    return DeviceType.Universal;
                }
                else if (iPhone || all) {
                    return DeviceType.IPhone;
                }
                else if (iPad) {
                    return DeviceType.IPad;
                }
                else {
                    return DeviceType.None;
                }
            }
            set {
                // 有些数据库要求属性必须可读+可写，这里留个空的setter
            }
        }

        public bool IsGameCenterEnabled {
            get {
                return Features != null && Features.Contains("gameCenter");
            }
            set {
                // 有些数据库要求属性必须可读+可写，这里留个空的setter
            }
        }

        public string PriceWithSymbol {
            get {
                string symbol = currencySymbolMapping.ContainsKey(Currency) ? currencySymbolMapping[Currency] : "￥";
                return symbol + Price;
            }
        }

        public void UpdateFrom(object obj) {
            AppBrief newOne = obj as AppBrief;
            if (newOne == null || newOne == this || newOne.Id != Id) {
                return;
            }

            Name = newOne.Name;
            Version = newOne.Version;
            ReleaseDate = newOne.ReleaseDate;
            Price = newOne.Price;
            Currency = newOne.Currency;
            FileSize = newOne.FileSize;
            ViewUrl = newOne.ViewUrl;
            IconUrl = newOne.IconUrl;
            Introduction = newOne.Introduction;
            ReleaseNotes = newOne.ReleaseNotes;
            PrimaryCategory = newOne.PrimaryCategory;
            Developer = newOne.Developer;
            AverageUserRatingForCurrentVersion = newOne.AverageUserRatingForCurrentVersion;
            UserRatingCountForCurrentVersion = newOne.UserRatingCountForCurrentVersion;
            Features = newOne.Features;
            SupportedDevices = newOne.SupportedDevices;
            if (newOne.LastValidUpdate != null) {
                LastValidUpdate = newOne.LastValidUpdate;
            }
            LanguagePriority = newOne.LanguagePriority;
            Hash = newOne.Hash;
        }
    }
}
