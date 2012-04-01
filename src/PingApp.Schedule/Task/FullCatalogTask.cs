using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using PingApp.Entity;
using System.Diagnostics;
using NLog;
using PingApp.Schedule.Storage;
using System.Configuration;

namespace PingApp.Schedule.Task {
    class FullCatalogTask : TaskNode {
        private readonly HashSet<int> set = new HashSet<int>();

        private readonly List<string> error = new List<string>();

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start analyzing full catalog");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Directory.CreateDirectory(LogRoot);
            IEnumerable<Category> categories = Program.Debug ? new Category[] { Category.Get(6001) } : Category.All;

            categories.AsParallel().ForAll(c => AyalyzeCategory(c));

            string[] extraUrls = {
                "http://itunes.apple.com/cn/genre/id6018?mt=8", "http://itunes.apple.com/cn/genre/id6000?mt=8",
                "http://itunes.apple.com/cn/genre/id6017?mt=8", "http://itunes.apple.com/cn/genre/id6016?mt=8",
                "http://itunes.apple.com/cn/genre/id6015?mt=8", "http://itunes.apple.com/cn/genre/id6014?mt=8",
                "http://itunes.apple.com/cn/genre/id7001?mt=8", "http://itunes.apple.com/cn/genre/id7002?mt=8",
                "http://itunes.apple.com/cn/genre/id7003?mt=8", "http://itunes.apple.com/cn/genre/id7004?mt=8",
                "http://itunes.apple.com/cn/genre/id7005?mt=8", "http://itunes.apple.com/cn/genre/id7006?mt=8",
                "http://itunes.apple.com/cn/genre/id7007?mt=8", "http://itunes.apple.com/cn/genre/id7008?mt=8",
                "http://itunes.apple.com/cn/genre/id7009?mt=8", "http://itunes.apple.com/cn/genre/id7010?mt=8",
                "http://itunes.apple.com/cn/genre/id7011?mt=8", "http://itunes.apple.com/cn/genre/id7012?mt=8",
                "http://itunes.apple.com/cn/genre/id7013?mt=8", "http://itunes.apple.com/cn/genre/id7014?mt=8",
                "http://itunes.apple.com/cn/genre/id7015?mt=8", "http://itunes.apple.com/cn/genre/id7016?mt=8",
                "http://itunes.apple.com/cn/genre/id7017?mt=8", "http://itunes.apple.com/cn/genre/id7018?mt=8",
                "http://itunes.apple.com/cn/genre/id7019?mt=8", "http://itunes.apple.com/cn/genre/id6013?mt=8",
                "http://itunes.apple.com/cn/genre/id6012?mt=8", "http://itunes.apple.com/cn/genre/id6020?mt=8",
                "http://itunes.apple.com/cn/genre/id6011?mt=8", "http://itunes.apple.com/cn/genre/id6010?mt=8",
                "http://itunes.apple.com/cn/genre/id6009?mt=8", "http://itunes.apple.com/cn/genre/id6008?mt=8",
                "http://itunes.apple.com/cn/genre/id6007?mt=8", "http://itunes.apple.com/cn/genre/id6006?mt=8",
                "http://itunes.apple.com/cn/genre/id6005?mt=8", "http://itunes.apple.com/cn/genre/id6004?mt=8",
                "http://itunes.apple.com/cn/genre/id6003?mt=8", "http://itunes.apple.com/cn/genre/id6002?mt=8",
                "http://itunes.apple.com/cn/genre/id6001?mt=8", "http://itunes.apple.com/cn/genre/id6022?mt=8"
            };
            extraUrls.AsParallel().ForAll(u => ParsePage(u, false));

            Log.Info("Analyze done, start rescue failed page");
            using (StreamWriter errorLog = new StreamWriter(Path.Combine(LogRoot, "error.txt"), true, Encoding.UTF8)) {
                error.AsParallel().ForAll(u => Rescue(u, errorLog));
            }

            watch.Stop();
            Log.Info("Work done using {0}min, founding {1} entries", watch.Elapsed.Minutes, set.Count);

            IStorage output = new MemoryStorage();
            output.Add(set);
            return output;
        }

        private void AyalyzeCategory(Category category) {
            string url = String.Format("http://itunes.apple.com/cn/genre/id{0}?mt=8", category.Id);

            Enumerable.Range((int)'A', 26).Concat(new int[] { (int)'*' })
                .AsParallel().ForAll(i => AnalyzeAlpha(url, category, (char)i));
        }

        private void AnalyzeAlpha(string baseUrl, Category category, char alpha) {
            string url = baseUrl + "&letter=" + alpha;
            ParseAlpha(url, false);
        }

        private void AnalyzePage(string baseUrl, int page) {
            string url = baseUrl + "&page=" + page;
            ParsePage(url, false);
        }

        private int GetPageCount(string url) {
            int page = 1;
            while (true) {
                string executingUrl = url + "&page=" + page;
                using (WebClient client = new WebClient()) {
                    client.Encoding = Encoding.UTF8;
                    string html = client.DownloadString(executingUrl);
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(html);

                    HtmlNode paginate = document.DocumentNode.SelectSingleNode("//ul[@class='list paginate']");

                    if (paginate == null) {
                        return 1;
                    }

                    // 找到内容是数字的
                    int lastPage = paginate.Descendants("a")
                        .Where(a => Regex.IsMatch(a.InnerHtml.Trim(), @"^\d+$"))
                        .Select(a => Convert.ToInt32(a.InnerHtml.Trim()))
                        .Last();
                    if (lastPage == page) {
                        // 如果最后还有“下一页”，则再加1
                        if (paginate.Descendants("a").Last().GetAttributeValue("class", String.Empty) == "paginate-more") {
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

        private void ParseAlpha(string url, bool throwOnError) {
            int pageCount;
            try {
                pageCount = GetPageCount(url);
            }
            catch (WebException ex) {
                Log.ErrorException(url + " failed", ex);
                if (throwOnError) {
                    throw;
                }
                else {
                    lock (error) {
                        error.Add(url);
                    }
                }
                return;
            }


            Log.Debug("{0} has {1} pages", url, pageCount);

            Enumerable.Range(1, pageCount).AsParallel().ForAll(i => AnalyzePage(url, i));
        }

        private void ParsePage(string url, bool throwOnError) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            using (WebClient client = new WebClient()) {
                client.Encoding = Encoding.UTF8;

                try {
                    string html = client.DownloadString(url);
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(html);
                    IEnumerable<HtmlNode> nodes = document.GetElementbyId("selectedcontent").Descendants("a");
                    List<int> list = new List<int>();
                    foreach (HtmlNode node in nodes) {
                        string name = node.InnerHtml.Trim();
                        string href = node.GetAttributeValue("href", String.Empty);
                        int id = Utility.ExtractIdFromUrl(href);
                        list.Add(id);
                        Log.Trace("{0} {1}", id, name);
                    }
                    lock (set) {
                        set.UnionWith(list);
                    }
                    watch.Stop();
                    Log.Info("{0} {1} {2}ms", url, nodes.Count(), watch.ElapsedMilliseconds);
                }
                catch (WebException ex) {
                    Log.ErrorException(url, ex);
                    if (throwOnError) {
                        throw;
                    }
                    else {
                        lock (error) {
                            error.Add(url);
                        }
                    }
                }
            }
        }

        private void Rescue(string url, StreamWriter errorLog) {
            if (url.Contains("&page=")) {
                RescuePage(url, errorLog);
            }
            else {
                RescueAlpha(url, errorLog);
            }
        }

        private void RescuePage(string url, StreamWriter errorLog) {
            for (int i = 0; i < 5; i++) {
                try {
                    ParsePage(url, true);
                    return;
                }
                catch (Exception ex) {
                    Log.WarnException(url, ex);
                }
            }

            Log.Error(url + " finally failed");
            lock (error) {
                errorLog.WriteLine(url);
            }
        }

        private void RescueAlpha(string url, StreamWriter errorLog) {
            for (int i = 0; i < 5; i++) {
                try {
                    ParseAlpha(url, true);
                    return;
                }
                catch (Exception ex) {
                    Log.WarnException(url, ex);
                }
            }

            Log.Error(url + " finally failed");
            lock (error) {
                errorLog.WriteLine(url);
            }
        }
    }
}
