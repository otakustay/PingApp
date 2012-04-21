using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using PanGu;
using PanGu.Match;
using PingApp.Entity;

namespace PingApp.Infrastructure {
    sealed class StandardAppParser : IAppParser {
        private const string CATEGORY_URL_TEMPLATE = "http://itunes.apple.com/cn/genre/id{0}?mt=8";

        private const string ALPHA_URL_TEMPLATE = CATEGORY_URL_TEMPLATE + "&letter={1}";

        private const string PAGE_URL_TEMPLATE = ALPHA_URL_TEMPLATE + "&page={2}";

        private const string SEARCH_API_URL_TEMPLATE = "http://itunes.apple.com/lookup?country=cn&&lang=zh_cn&id={0}";

        // 26个字母，外加一个表示“其它”的*
        private static readonly IEnumerable<char> CATALOG_ALPHAS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ*";

        private static readonly Category[] categoriesForDebug = { Category.Get(6001) };

        private readonly IWebDownload download;

        private readonly JsonSerializerSettings serializerSettings;

        private readonly int truncateLimit;

        private readonly MatchOptions segmentMatchOptions;

        private readonly ProgramSettings settings;

        private readonly Logger logger;

        private readonly ISet<int> output;

        public StandardAppParser(IWebDownload download, JsonSerializerSettings serializerSettings, 
            int truncateLimit, MatchOptions segmentMatchOptions, ProgramSettings settings, Logger logger) {
            this.download = download;
            this.serializerSettings = serializerSettings;
            this.truncateLimit = truncateLimit;
            this.segmentMatchOptions = segmentMatchOptions;
            this.settings = settings;
            this.logger = logger;

            // Debug下为了Fiddler的Auto Responder能稳定拦截请求，需要对id进行排序
            if (settings.Debug) {
                this.output = new SortedSet<int>();
            }
            else {
                this.output = new HashSet<int>();
            }
        }

        public ISet<int> CollectAppsFromCatalog() {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            logger.Info("Start parse catalogs");

            /*
             * 1. 一个分类有A-Z共26个字母，外加1个“其他”，用*表示
             * 2. 每个分类+字母的组合会有若干页，要取得具体的页数
             * 3. 每页有若干个应用，每个应用是一个<a>元素
             */

            IEnumerable<Category> categories = settings.Debug ? categoriesForDebug : Category.All;
            categories.AsParallel()
                .WithDegreeOfParallelism(settings.ParallelDegree)
                .ForAll(c => FromCategory(c));

            IEnumerable<Category> extrayCategories = Category.All.Concat(Category.Games);
            extrayCategories.AsParallel()
                .WithDegreeOfParallelism(settings.ParallelDegree)
                .ForAll(c => FromPage(c, Char.MinValue, 0));

            watch.Stop();
            logger.Info("Collected {0} apps using {1}", output.Count, watch.Elapsed);

            return output;
        }

        public ISet<int> CollectAppsFromRss(string url) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            XDocument document = download.AsXml(url);
            IEnumerable<int> retrieved = document.Root.Descendants("{http://www.w3.org/2005/Atom}entry")
                .Select(d => d.Elements("{http://www.w3.org/2005/Atom}id").First().Value)
                .Select(Utility.FindIdFromUrl);
            ISet<int> apps = new HashSet<int>(retrieved);

            watch.Stop();
            logger.Info("Found {0} apps from rss feed using {1}ms", apps.Count, watch.ElapsedMilliseconds);

            return apps;
        }

        public ICollection<App> RetrieveApps(ICollection<int> required, int attempts = 0) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            string idx = String.Join(",", required);
            string url = String.Format(SEARCH_API_URL_TEMPLATE, idx);
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

        #region For CollectAppsFromCatalog

        private void FromCategory(Category category) {
            CATALOG_ALPHAS.AsParallel()
                .WithDegreeOfParallelism(settings.ParallelDegree)
                .ForAll(a => FromAlpha(category, a));
        }

        private void FromAlpha(Category category, char alpha, int attempts = 0) {
            string url = String.Format(ALPHA_URL_TEMPLATE, category.Id, alpha);

            try {
                int pageCount = GetPageCount(category, alpha);

                logger.Debug(
                    "There are {0} pages in category {1}-{2}, alpha {3}",
                    pageCount, category.Id, category.Name, alpha
                );

                Enumerable.Range(1, pageCount).AsParallel()
                    .WithDegreeOfParallelism(settings.ParallelDegree)
                    .ForAll(i => FromPage(category, alpha, i));
            }
            catch (WebException ex) {
                string logMessage = String.Format(
                    "Failed to get page count for category {0}-{1}, alpha {2}",
                    category.Id, category.Name, alpha
                );


                if (attempts >= settings.RetryAttemptCount) {
                    logger.ErrorException(logMessage, ex);
                }
                else {
                    logger.WarnException(logMessage, ex);
                }
            }
        }

        private void FromPage(Category category, char alpha, int pageIndex, int attempts = 0) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            string url = alpha == Char.MinValue ?
                String.Format(CATEGORY_URL_TEMPLATE, category.Id) :
                String.Format(PAGE_URL_TEMPLATE, category.Id, alpha, pageIndex);
            try {
                HtmlDocument document = download.AsDocument(url);
                IEnumerable<HtmlNode> nodes = document.GetElementbyId("selectedcontent").Descendants("a");
                lock (output) {
                    foreach (HtmlNode node in nodes) {
                        string name = node.InnerHtml.Trim();
                        string href = node.GetAttributeValue("href", String.Empty);
                        int id = Utility.FindIdFromUrl(href);
                        output.Add(id);
                        logger.Trace("Found app {0}-{1}", id, name);
                    }
                }
                watch.Stop();
                logger.Debug("Found {0} apps in {1} using {2}ms", nodes.Count(), url, watch.ElapsedMilliseconds);
            }
            catch (WebException ex) {
                string logMessage = alpha == Char.MinValue ?
                    String.Format(
                        "Failed to extract apps for category {0}-{1}, alpha {2}, page {3}",
                        category.Id, category.Name, alpha, pageIndex
                    ) :
                    String.Format(
                        "Fail to extract apps from special page for category {0}-{1}",
                        category.Id, category.Name
                    );

                if (attempts >= settings.RetryAttemptCount) {
                    logger.ErrorException(logMessage, ex);
                }
                else {
                    logger.WarnException(logMessage, ex);
                    FromPage(category, alpha, pageIndex, attempts + 1);
                }
            }
        }

        private int GetPageCount(Category category, char alpha) {
            int page = 1;
            while (true) {
                string url = String.Format(PAGE_URL_TEMPLATE, category.Id, alpha, page);
                HtmlDocument document = download.AsDocument(url);

                HtmlNode pager = document.DocumentNode.SelectSingleNode("//ul[@class='list paginate']");

                if (pager == null) {
                    return 1;
                }

                // 找到内容是数字的
                int lastPage = pager.Descendants("a")
                    .Where(a => Regex.IsMatch(a.InnerHtml.Trim(), @"^\d+$"))
                    .Select(a => Convert.ToInt32(a.InnerHtml.Trim()))
                    .Last();
                if (lastPage == page) {
                    // 如果最后还有“下一页”，则再加1
                    if (pager.Descendants("a").Last().GetAttributeValue("class", String.Empty) == "paginate-more") {
                        page++;
                    }
                    return page;
                }
                else {
                    page = lastPage;
                }
            }
        }

        #endregion

        #region For RetrieveApps

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

        #endregion
    }
}
