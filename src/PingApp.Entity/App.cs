using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PingApp.Entity {
    public class App : IUpdateTarget {
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

        public void UpdateFrom(object obj) {
            App newOne = obj as App;
            if (newOne == null || newOne == this || newOne.Id != Id) {
                return;
            }

            Description = newOne.Description;
            LargeIconUrl = newOne.LargeIconUrl;
            Seller = newOne.Seller;
            ReleaseNotes = newOne.ReleaseNotes;
            CensoredName = newOne.CensoredName;
            ContentAdvisoryRating = newOne.ContentAdvisoryRating;
            ContentAdvisoryRating = newOne.ContentAdvisoryRating;
            AverageUserRating = newOne.AverageUserRating;
            UserRatingCount = newOne.UserRatingCount;
            Languages = newOne.Languages;
            Categories = newOne.Categories;
            ScreenshotUrls = newOne.ScreenshotUrls;
            IPadScreenshotUrls = newOne.IPadScreenshotUrls;
            Brief.UpdateFrom(newOne.Brief);
        }
    }
}
