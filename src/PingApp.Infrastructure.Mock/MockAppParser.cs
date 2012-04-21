using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Infrastructure.Mock {
    public sealed class MockAppParser : IAppParser {
        private readonly IEnumerable<int> data;

        public MockAppParser(IEnumerable<int> identities) {
            data = identities;
        }

        public MockAppParser(int start = 1, int count = 10) {
            data = Enumerable.Range(start, count);
        }

        public ISet<int> CollectAppsFromCatalog() {
            return new HashSet<int>(data);
        }

        public ISet<int> CollectAppsFromRss(string url) {
            return new HashSet<int>(data);
        }

        public ICollection<App> RetrieveApps(ICollection<int> required, int attempts = 0) {
            return required.Select(i => GetTemplatedApp(i)).ToArray();
        }

        private static App GetTemplatedApp(int id) {
            return new App() {
                Id = id,
                UserRatingCount = 320,
                AverageUserRating = 4.5f,
                CensoredName = "test",
                ContentAdvisoryRating = "4+",
                ContentRating = "6+",
                ReleaseNotes = "this is a test release notes",
                Description = "this is a test paragraph",
                LargeIconUrl = "http://url.for.test/large-icon.png",
                IPadScreenshotUrls = new string[] {
                    "http://url.for.test/ipad/1.png",
                    "http://url.for.test/ipad/2.png",
                    "http://url.for.test/ipad/3.png"
                },
                ScreenshotUrls = new string[] {
                    "http://url.for.test/iphone/1.png",
                    "http://url.for.test/iphone/2.png",
                    "http://url.for.test/iphone/3.png"
                },
                Languages = new string[] {
                    "cn", "en", "ge", "jp"
                },
                Categories = new Category[] {
                    Category.Get(6001),
                    Category.Get(6002),
                    Category.Get(6003),
                    Category.Get(6004),
                },
                Seller = new Seller() {
                    Name = "test seller",
                    ViewUrl = "http://url.for.test/seller/test"
                },
                Brief = new AppBrief() {
                    Id = id,
                    Name = "test",
                    Version = "3.1.2",
                    ViewUrl = "http://url.for.test/test",
                    UserRatingCountForCurrentVersion = 53,
                    AverageUserRatingForCurrentVersion = 4f,
                    Currency = "CNY",
                    Price = 18,
                    PrimaryCategory = Category.Get(6001),
                    ReleaseDate = new DateTime(2011, 03, 07),
                    ReleaseNotes = "this is a test release notes",
                    SupportedDevices = new string[] { "all" },
                    DeviceType = DeviceType.Universal,
                    Features = new string[] {
                        "gameCenter",
                        "iosUniversal"
                    },
                    FileSize = 83135612,
                    IconUrl = "http://url.for.test/icon.png",
                    Introduction = "this is a test introduction paragraph",
                    LanguagePriority = 100,
                    Developer = new Developer() {
                        Id = 20135134, 
                        Name = "test developer", 
                        ViewUrl = "http://url.for.test/developer/test"
                    }
                    // LastValidUpdate不提供
                }
            };
        }
    }
}
