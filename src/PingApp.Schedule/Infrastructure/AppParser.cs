using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using PanGu;
using PanGu.Match;
using PingApp.Entity;

namespace PingApp.Schedule.Infrastructure {
    sealed class AppParser {
        private const string URL_TEMPLATE = "http://itunes.apple.com/lookup?country=cn&&lang=zh_cn&id={0}";

        private readonly WebDownload download;

        private readonly JsonSerializerSettings serializerSettings;

        private readonly int truncateLimit;

        private readonly MatchOptions segmentMatchOptions;

        private readonly ProgramSettings settings;

        private readonly Logger logger;

        public AppParser(WebDownload download, JsonSerializerSettings serializerSettings, 
            int truncateLimit, MatchOptions segmentMatchOptions, ProgramSettings settings, Logger logger) {
            this.download = download;
            this.serializerSettings = serializerSettings;
            this.truncateLimit = truncateLimit;
            this.segmentMatchOptions = segmentMatchOptions;
            this.settings = settings;
            this.logger = logger;
        }

        public ICollection<App> RetrieveApps(IEnumerable<int> required, int attempts = 0) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            string idx = String.Join(",", required);
            string url = String.Format(URL_TEMPLATE, idx);
            try {
                JObject json = download.AsJson(url);
                IEnumerable<JToken> results = json["results"].Children();
                ICollection<App> apps = results.Select(ParseApp).ToArray();

                watch.Stop();
                logger.Debug("Retrieved {0} apps using {1}ms", apps.Count, watch.ElapsedMilliseconds);
                int notFound = required.Count() - apps.Count;
                if (notFound > 0) {
                    logger.Debug("There are {0} required but not found in search api", notFound);
                }

                return apps;
            }
            catch (WebException ex) {
                string logMessage = String.Format("Failed to download these apps from search api: {0}", idx);
                if (attempts < settings.RetryAttemptCount) {
                    logger.Warn(logMessage, ex);
                    return RetrieveApps(required, attempts + 1);
                }
                else {
                    logger.ErrorException(logMessage, ex);
                    return null;
                }
            }
            catch (JsonSerializationException ex) {
                logger.ErrorException(String.Format("Failed to parse these apps: {0}", idx), ex);
                return null;
            }
        }

        private App ParseApp(JToken token) {
            string json = token.ToString();
            string artworkUrl = token["artworkUrl100"].Value<string>() ?? String.Empty;
            int offset = artworkUrl.LastIndexOf('.');
            // itunes的图片有规律
            string iconUrl = artworkUrl.Substring(0, offset) + ".100x100-75" + artworkUrl.Substring(offset);
            string largeIconUrl = artworkUrl.Substring(0, offset) + ".175x175-75" + artworkUrl.Substring(offset);

            // 先按规则把能解析的都解析了
            AppBrief brief = JsonConvert.DeserializeObject<AppBrief>(json, serializerSettings);
            App app = JsonConvert.DeserializeObject<App>(json, serializerSettings);
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
            brief.Introduction = TruncateParagraph(app.Description);
            brief.ReleaseNotes = TruncateParagraph(app.ReleaseNotes);
            brief.LanguagePriority = app.LanguagePriority;
            brief.PrimaryCategory = Category.Get(token["primaryGenreId"].Value<int>());
            brief.Developer = new Developer(
                token["artistId"].Value<int>(),
                token["artistName"].Value<string>() ?? String.Empty,
                token["artistViewUrl"] == null ? String.Empty : token["artistViewUrl"].Value<string>() ?? String.Empty
            );
            brief.Hash = App.ComputeHash(app);
            // 能在Search API上找的都是有效的
            brief.IsActive = true;

            return app;
        }

        private string TruncateParagraph(string paragraph) {
            Segment segment = new Segment();
            ICollection<WordInfo> words = segment.DoSegment(paragraph, segmentMatchOptions);
            int stop = 0;
            // 最短不能小于需要长度的1/2
            int lowerBound = truncateLimit / 2;
            foreach (WordInfo word in words) {
                if (word.Position + word.Word.Length > truncateLimit) {
                    return stop < lowerBound ? 
                        paragraph.Substring(0, truncateLimit) : 
                        paragraph.Substring(0, stop);
                }
                stop = word.Position + word.Word.Length;
            }
            return paragraph;
        }
    }
}
