using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System.Configuration;
using System.IO;
using PingApp.Utility.Lucene;
using PingApp.Entity;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Data;
using PingApp.Utility;

namespace PingApp.Schedule.Task {
    class TestSearchTask : TaskNode {
        private readonly IEnumerable<string> args;

        public TestSearchTask(IEnumerable<string> args) {
            this.args = args;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start search on lucene index using " + String.Join(" ", args));
            Stopwatch watch = new Stopwatch();
            watch.Start();

            DirectoryInfo directory = new DirectoryInfo(ConfigurationManager.AppSettings["LuceneIndexDirectory"]);
            AppQuery query = new AppQuery();
            foreach (string pattern in args) {
                string[] items = pattern.Split('=');
                switch (items[0]) {
                    case "--name":
                        query.WithKeywords(items[1].Replace(',', ' '));
                        break;
                    case "--category":
                        query.WithCategory(Convert.ToInt32(items[1]));
                        break;
                    case "--device":
                        query.WithDeviceType((DeviceType)Enum.Parse(typeof(DeviceType), Utility.Capitalize(items[1])));
                        break;
                    case "--language":
                        query.WithLanguagePriority(Convert.ToInt32(items[1]));
                        break;
                    case "--sort":
                        query.SortBy((AppSortType)Enum.Parse(typeof(AppSortType), Utility.Capitalize(items[1])), false);
                        break;
                    default:
                        break;
                }
            }

            IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(directory), true);
            TopDocs docs = searcher.Search(query.Query, null, 50, query.Sort);
            int[] found = docs.scoreDocs
                .Select(d => searcher.Doc(d.doc).GetField("Id").StringValue())
                .Select(s => Convert.ToInt32(s))
                .ToArray();

            watch.Stop();
            Log.Info("{0} apps found using {1}ms", found.Length, watch.ElapsedMilliseconds);
            Log.Info("Start retrieve data from db");
            watch.Start();

            if (found.Length > 0) {
                AppBrief[] apps = GetApps(found);
                PrettyPrint(apps);
            }

            watch.Stop();
            Log.Info("Work done using {0}ms", watch.ElapsedMilliseconds);

            return null;
        }

        private AppBrief[] GetApps(int[] found) {
            List<AppBrief> apps = new List<AppBrief>();
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                MySqlCommand cmd = connection.CreateCommand();
                cmd.CommandText =
@"select Id, Name, DeveloperName, PrimaryCategory, Features, LanguagePriority, 
    SupportedDevices, LastValidUpdateTime, Price, AverageUserRatingForCurrentVersion 
from AppBrief where Id in (" + String.Join(",", found) + ");";
                using (IDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        AppBrief app = new AppBrief() {
                            Id = reader.Get<int>("Id"),
                            Name = reader.Get<string>("Name"),
                            Developer = new Developer() { Name = reader.Get<string>("DeveloperName") },
                            PrimaryCategory = Category.Get(reader.Get<int>("PrimaryCategory")),
                            LastValidUpdate = new AppUpdate() { Time = reader.Get<DateTime>("LastValidUpdateTime") },
                            AverageUserRatingForCurrentVersion = reader.Get<float?>("AverageUserRatingForCurrentVersion"),
                            Features = reader.Get<string>("Features")
                                .Split(',').Where(s => s.Length > 0).ToArray(),
                            LanguagePriority = reader.Get<int>("LanguagePriority"),
                            SupportedDevices = reader.Get<string>("SupportedDevices")
                                .Split(',').Where(s => s.Length > 0).ToArray()
                        };
                        apps.Add(app);
                    }
                }
            }
            apps.Sort((x, y) => (Array.IndexOf(found, x.Id) - Array.IndexOf(found, y.Id)));

            return apps.ToArray();
        }

        private void PrettyPrint(AppBrief[] apps) {
            int[] fields = {
                Math.Max("Id".Length, apps.Select(a => a.Id.ToString()).Max(s => s.Length)),
                Math.Max("Name".Length, apps.Select(a => a.Name).Max(s => s.Length)),
                Math.Max("DeveloperName".Length, apps.Select(a => a.Developer.Name).Max(s => s.Length)),
                Math.Max("PrimaryCategory".Length, apps.Select(a => a.PrimaryCategory.Id.ToString()).Max(s => s.Length)),
                Math.Max("DeviceType".Length, apps.Select(a => a.DeviceType.ToString()).Max(s => s.Length)),
                Math.Max("LastValidUpdateTime".Length, apps.Select(a => a.LastValidUpdate.Time.ToString("yyyy-MM-dd HH:mm:ss")).Max(s => s.Length)),
                Math.Max("Price".Length, apps.Select(a => a.Price.ToString()).Max(s => s.Length)),
                Math.Max("RatingCount".Length, apps.Select(a => a.AverageUserRatingForCurrentVersion.ToString()).Max(s => s.Length)),
                Math.Max("LanguagePriority".Length, apps.Select(a => a.LanguagePriority.ToString()).Max(s => s.Length))
            };
            int totalLength = fields.Sum() + 28;
            string template = "| {0,-" + fields[0] + "} | " + 
                "{1,-" + fields[1] + "} | " + 
                "{2,-" + fields[2] + "} | " + 
                "{3,-" + fields[3] + "} | " + 
                "{4,-" + fields[4] + "} | " + 
                "{5,-" + fields[5] + "} | " + 
                "{6,-" + fields[6] + "} | " + 
                "{7,-" + fields[7] + "} | " +
                "{8,-" + fields[8] + "} |";

            string path = Path.Combine(LogRoot, "result.txt");
            using (StreamWriter output = new StreamWriter(path, false, Encoding.UTF8)) {
                output.WriteLine(String.Join(" ", args));

                string separator = String.Join(String.Empty, Enumerable.Repeat("-", totalLength).ToArray());
                output.WriteLine(separator);
                output.WriteLine(template, "Id", "Name", "Developer", "Category", "DeviceType", "LastUpdate", "Price", "RatingCount", "LanguagePriority");
                output.WriteLine(separator);
                foreach (AppBrief app in apps) {
                    output.WriteLine(
                        template, app.Id, app.Name, app.Developer.Name, app.PrimaryCategory.Id, app.DeviceType, 
                        app.LastValidUpdate.Time, app.Price, app.UserRatingCountForCurrentVersion, app.LanguagePriority
                    );
                }
                output.WriteLine(separator);
            }

            Log.Info("Result saved to {0}", path);

            Process.Start("notepad", path);
        }
    }
}
