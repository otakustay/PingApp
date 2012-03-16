using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace PingApp.Schedule.Task {
    class Top100CheckTask : TaskNode {
        private static readonly Regex selector = new Regex(@"<id>([^<]+)</id>", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly HashSet<int> result = new HashSet<int>();

        protected override IStorage RunTask(IStorage input) {
            string freeUrlTemplate = "http://itunes.apple.com/cn/rss/topfreeapplications/limit=100/genre={0}/xml";
            string paidUrlTemplate = "http://itunes.apple.com/cn/rss/toppaidapplications/limit=100/genre={0}/xml";
            string hotUrlTemplate = "http://itunes.apple.com/cn/rss/topgrossingapplications/limit=100/genre={0}/xml";
            string freeIPadUrlTemplate = "http://itunes.apple.com/cn/rss/topfreeipadapplications/limit=100/genre={0}/xml";
            string paidIPadUrlTemplate = "http://itunes.apple.com/cn/rss/toppaidipadapplications/limit=100/genre={0}/xml";
            string hotIPadUrlTemplate = "http://itunes.apple.com/cn/rss/topgrossingipadapplications/limit=100/genre={0}/xml";

            List<string> urls = new List<string>() {
                "http://itunes.apple.com/cn/rss/topfreeapplications/limit=100/xml",
                "http://itunes.apple.com/cn/rss/toppaidapplications/limit=100/xml",
                "http://itunes.apple.com/cn/rss/topgrossingapplications/limit=100/xml",
                "http://itunes.apple.com/cn/rss/topfreeipadapplications/limit=100/xml",
                "http://itunes.apple.com/cn/rss/toppaidipadapplications/limit=100/xml",
                "http://itunes.apple.com/cn/rss/topgrossingipadapplications/limit=100/xml"
            };
            urls.AddRange(Category.All.Select(c => String.Format(freeUrlTemplate, c.Id)));
            urls.AddRange(Category.All.Select(c => String.Format(paidUrlTemplate, c.Id)));
            urls.AddRange(Category.All.Select(c => String.Format(hotUrlTemplate, c.Id)));
            urls.AddRange(Category.All.Select(c => String.Format(freeIPadUrlTemplate, c.Id)));
            urls.AddRange(Category.All.Select(c => String.Format(paidIPadUrlTemplate, c.Id)));
            urls.AddRange(Category.All.Select(c => String.Format(hotIPadUrlTemplate, c.Id)));

            Log.Info("Start scrap top100 urls");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            urls.AsParallel().ForAll(u => ScrapResult(u));

            watch.Stop();
            Log.Info("{0} apps retrieved using {1}ms", result.Count, watch.ElapsedMilliseconds);

            if (!Program.Debug && result.Count > 0) {
                string url = "http://localhost/UpdateTop100.ashx";
                using (WebClient client = new WebClient()) {
                    client.Encoding = Encoding.UTF8;
                    string serverReturn = client.UploadString(url, String.Join(",", result));
                    if (serverReturn == "true") {
                        Log.Info("Successfully upload to web server");
                    }
                    else {
                        Log.Error("Upload to web server failed: {0}", serverReturn);
                    }
                }
            }

            return null;
        }

        private void ScrapResult(string url) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < 5; i++) {
                try {
                    using (WebClient client = new WebClient()) {
                        client.Encoding = Encoding.UTF8;
                        string xml = client.DownloadString(url);
                        int[] products = selector.Matches(xml)
                            .Cast<Match>()
                            .Select(m => m.Groups[1].Value)
                            .Select(s => Utility.ExtractIdFromUrl(s))
                            .Where(d => d != -1)
                            .ToArray();
                        lock (result) {
                            result.UnionWith(products);
                        }

                        watch.Stop();
                        Log.Debug("{0} - {1}", url, products.Length);
                        return;
                    }
                }
                catch (WebException ex) {
                    Log.WarnException(String.Format("Load {0} failed", url), ex);
                }
                catch (Exception ex) {
                    Log.ErrorException(String.Format("Parse {0} failed", url), ex);
                    throw;
                }
            }

            Log.Error("Load {0} failed", url);
        }
    }
}
