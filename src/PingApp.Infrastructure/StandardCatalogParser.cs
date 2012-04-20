using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NLog;
using PingApp.Entity;
using PingApp.Infrastructure;

namespace PingApp.Infrastructure {
    sealed class StandardCatalogParser : ICatalogParser {
        private const string CATEGORY_URL_TEMPLATE = "http://itunes.apple.com/cn/genre/id{0}?mt=8";

        private const string ALPHA_URL_TEMPLATE = CATEGORY_URL_TEMPLATE + "&letter={1}";

        private const string PAGE_URL_TEMPLATE = ALPHA_URL_TEMPLATE + "&page={2}";

        private static readonly IEnumerable<char> alphas =
            Enumerable.Range((int)'A', 26).Concat(new int[] { (int)'*' }).Select(i => (char)i);

        private static readonly Category[] categoriesForDebug = { Category.Get(6001) };

        private readonly IWebDownload download;

        private readonly ProgramSettings settings;

        private readonly Logger logger;

        private readonly ISet<int> output;

        public StandardCatalogParser(IWebDownload download, ProgramSettings settings, Logger logger) {
            this.download = download;
            this.settings = settings;
            this.logger = logger;
            if (settings.Debug) {
                this.output = new SortedSet<int>();
            }
            else {
                this.output = new HashSet<int>();
            }
        }

        public ISet<int> CollectApps() {
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

        private void FromCategory(Category category) {
            alphas.AsParallel()
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
    }
}
