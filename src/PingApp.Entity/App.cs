using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PingApp.Entity {
    public class App {
        public int Id { get; set; }

        public string Description { get; set; }

        public string LargeIconUrl { get; set; }

        public Seller Seller { get; set; }

        public string ReleaseNotes { get; set; }

        public string CensoredName { get; set; }

        public string ContentRating { get; set; }

        public string ContentAdvisoryRating { get; set; }

        public float? AverageUserRating { get; set; }

        public int? UserRatingCount { get; set; }

        public string[] Languages { get; set; }

        public Category[] Categories { get; set; }

        public string[] ScreenshotUrls { get; set; }

        public string[] IPadScreenshotUrls { get; set; }

        public int LanguagePriority {
            get {
                HashSet<string> set = new HashSet<string>(Languages);
                if (set.Contains("ZH")) {
                    return 1000;
                }
                else if (set.Contains("EN")) {
                    return 100;
                }
                else if (set.Contains("JA") || set.Contains("KO")) {
                    return 10;
                }
                else {
                    return 1;
                }
            }
        }

        public AppBrief Brief { get; set; }

        public App() {
            Description = String.Empty;
            LargeIconUrl = String.Empty;
            ReleaseNotes = String.Empty;
            CensoredName = String.Empty;
            ContentRating = String.Empty;
            ContentAdvisoryRating = String.Empty;
        }

        public override bool Equals(object obj) {
            App other = obj as App;
            if (other == null) {
                return false;
            }

            EqualsBuilder builder = new EqualsBuilder();
            builder.Append(Id, other.Id);
            builder.Append(Description, other.Description);
            builder.Append(ReleaseNotes, other.ReleaseNotes);
            builder.Append(CensoredName, other.CensoredName);
            builder.Append(ContentRating, other.ContentRating);
            builder.Append(ContentAdvisoryRating, other.ContentAdvisoryRating);
            builder.Append(AverageUserRating, other.AverageUserRating);
            builder.Append(UserRatingCount, other.UserRatingCount);
            builder.Append(Seller, other.Seller);
            builder.Append(Brief, other.Brief);
            builder.AppendSequence(Languages, other.Languages);
            builder.AppendSequence(Categories, other.Categories);
            builder.AppendSequence(ScreenshotUrls, other.ScreenshotUrls);
            builder.AppendSequence(IPadScreenshotUrls, other.IPadScreenshotUrls);
            return builder.AreEqual;
        }

        public override int GetHashCode() {
            return Id;
        }

        public override string ToString() {
            return "App: " + Id;
        }

        public static string ComputeHash(App app) {

            // 所有参与app的Equals运算的都加入
            string all = String.Empty;
            all += app.AverageUserRating.ToString();
            all += String.Join(",", app.Categories.Select(c => c.Id).OrderBy(i => i));
            all += app.CensoredName;
            all += app.ContentAdvisoryRating;
            all += app.ContentRating;
            all += app.Description;
            all += app.Id.ToString();
            all += String.Join(",", app.IPadScreenshotUrls.OrderBy(s => s));
            all += String.Join(",", app.Languages.OrderBy(s => s));
            all += app.LargeIconUrl;
            all += app.ReleaseNotes;
            all += String.Join(",", app.ScreenshotUrls.OrderBy(s => s));
            all += app.Seller.Name;
            all += app.Seller.ViewUrl;
            all += app.UserRatingCount.ToString();
            all += app.Brief.AverageUserRatingForCurrentVersion.ToString();
            all += app.Brief.Currency;
            all += app.Brief.Developer.Id.ToString();
            all += app.Brief.Developer.Name;
            all += app.Brief.Developer.ViewUrl;
            all += String.Join(",", app.Brief.Features.OrderBy(s => s));
            all += app.Brief.FileSize.ToString();
            all += app.Brief.IconUrl;
            all += app.Brief.Introduction;
            all += app.Brief.LanguagePriority.ToString();
            all += app.Brief.Name;
            all += app.Brief.Price.ToString();
            all += app.Brief.PrimaryCategory.Id.ToString();
            all += app.Brief.ReleaseDate.ToString("yyyyMMddHHmmss");
            all += app.Brief.ReleaseNotes;
            all += String.Join(",", app.Brief.SupportedDevices.OrderBy(s => s));
            all += app.Brief.UserRatingCountForCurrentVersion.ToString();
            all += app.Brief.Version;
            all += app.Brief.ViewUrl;

            // String的Hashcode在各版本的.net和各系统的.net下都不同，还是用md5吧
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(all));
            return BitConverter.ToString(bytes).Replace("-", String.Empty);
        }
    }
}
