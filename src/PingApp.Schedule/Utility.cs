using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PingApp.Entity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Data;
using NLog.Config;
using NLog;
using NLog.Targets;
using System.IO;
using PingApp.Utility;
using System.Security.Cryptography;

namespace PingApp.Schedule {
    static class Utility {
        private static readonly Regex idFromUrl = new Regex(@"\/id(\d+)", RegexOptions.Compiled);
        public static int ExtractIdFromUrl(string url) {
            Match match = idFromUrl.Match(url);
            return (match != null && match.Groups.Count >= 2) ? Convert.ToInt32(match.Groups[1].Value) : -1;
        }

        public static string Capitalize(string s) {
            string[] items = s.Split('-');
            string result = String.Empty;
            foreach (string item in items) {
                result += Char.ToUpper(item[0]) + item.Substring(1);
            }
            return result;
        }

        public static IEnumerable<T[]> Partition<T>(IEnumerable<T> input, int size) {
            int count = 0;
            List<T> list = new List<T>(size);
            foreach (T item in input) {
                count++;
                list.Add(item);
                if (count % size == 0) {
                    yield return list.ToArray();
                    list = new List<T>(size);
                }
            }
            if (list.Count > 0) {
                yield return list.ToArray();
            }
        }

        public static App[] ParseSearchApiResponse(string response) {
            List<App> apps = new List<App>(200);
            JObject json = JObject.Parse(response);
            foreach (JToken token in json["results"].Children()) {
                string s = token.ToString();
                string artworkUrl = token["artworkUrl100"].Value<string>() ?? String.Empty;
                int offset = artworkUrl.LastIndexOf('.');
                // itunes的图片有规律
                string iconUrl = artworkUrl.Substring(0, offset) + ".100x100-75" + artworkUrl.Substring(offset);
                string largeIconUrl = artworkUrl.Substring(0, offset) + ".175x175-75" + artworkUrl.Substring(offset);

                AppBrief brief = Utility.JsonDeserialize<AppBrief>(s);
                App app = Utility.JsonDeserialize<App>(s);
                // 额外字段
                app.Id = token["trackId"].Value<int>();
                app.LargeIconUrl = largeIconUrl;
                app.CensoredName = token["trackCensoredName"].Value<string>() ?? String.Empty;
                app.ContentRating = token["trackContentRating"].Value<string>() ?? String.Empty;
                app.Languages = token["languageCodesISO2A"].Values<string>().ToArray();
                app.Seller = new Seller(
                    token["sellerName"].Value<string>() ?? String.Empty,
                    token["sellerUrl"] == null ? String.Empty : token["sellerUrl"].Value<string>() ?? String.Empty
                );
                app.Categories = token["genreIds"].Values<int>().Select(i => Category.Get(i)).Where(c => c != null).ToArray();
                app.Brief = brief;
                if (app.ContentRating == "Not yet rated") {
                    app.ContentRating = String.Empty;
                }

                brief.Id = app.Id;
                brief.Name = token["trackName"].Value<string>() ?? String.Empty;
                brief.FileSize = token["fileSizeBytes"].Value<int>();
                brief.ViewUrl = token["trackViewUrl"].Value<string>() ?? String.Empty;
                brief.IconUrl = iconUrl;
                brief.Introduction = app.Description.SplitWordTo(200);
                brief.ReleaseNotes = app.ReleaseNotes.SplitWordTo(200);
                brief.LanguagePriority = GetLanguagePriority(app.Languages);
                brief.PrimaryCategory = Category.Get(token["primaryGenreId"].Value<int>());
                brief.Developer = new Developer(
                    token["artistId"].Value<int>(),
                    token["artistName"].Value<string>() ?? String.Empty,
                    token["artistViewUrl"] == null ? String.Empty : token["artistViewUrl"].Value<string>() ?? String.Empty
                );
                brief.Hash = Utility.ComputeAppHash(app);

                apps.Add(app);
            }

            return apps.ToArray();
        }

        public static Logger GetLogger(string logRoot, string name) {

            LoggingConfiguration config = new LoggingConfiguration();

            ColoredConsoleTarget console = new ColoredConsoleTarget();
            config.AddTarget("console", console);
            FileTarget file = new FileTarget();
            config.AddTarget("file", file);
            FileTarget debug = new FileTarget();
            config.AddTarget("debug", debug);
            FileTarget trace = new FileTarget();
            config.AddTarget("trace", trace);
            FileTarget error = new FileTarget();
            config.AddTarget("error", error);

            console.Layout = "${time}|${level}|${message}" + (Program.Debug ? "\n${exception:format=tostring}" : String.Empty);
            file.FileName = logRoot + "/log.txt";
            file.Layout = "${time}|${level}|${message}\n${exception:format=tostring}";
            debug.FileName = logRoot + "/debug.txt";
            debug.Layout = "${time}|${level}|${message}\n${exception:format=tostring}";
            trace.FileName = logRoot + "/verbose.txt";
            trace.Layout = "${time}|${level}|${message}\n${exception:format=tostring}";
            error.FileName = Directory.GetParent(logRoot).FullName + "/error.txt";
            error.Layout = "${time}|${level}|${message}\n${exception:format=tostring}";

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, console));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, file));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debug));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, trace));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, error));

            LogManager.Configuration = config;

            return LogManager.GetLogger(name);
        }

        public static string JsonSerialize(object value) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            string text = JsonConvert.SerializeObject(value, Formatting.None, settings);
            return text;
        }

        public static T JsonDeserialize<T>(string text) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            return JsonConvert.DeserializeObject<T>(text, settings);
        }

        public static string ComputeAppHash(App app) {
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

        private static int GetLanguagePriority(string[] languages) {
            HashSet<string> set = new HashSet<string>(languages);
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
}
