using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using NLog;
using PingApp.Entity;
using System.Diagnostics;
using PingApp.Schedule.Storage;

namespace PingApp.Schedule.Task {
    class SearchApiTask : TaskNode {
        private readonly bool computeDiff;

        private readonly List<int> notFound = new List<int>();

        public SearchApiTask(bool computeDiff) {
            this.computeDiff = computeDiff;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start scraping from search api using --compute-diff={0}", computeDiff);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Directory.CreateDirectory(LogRoot);
            IStorage output = Action == ActionType.RssCheck ?
                (IStorage)new MemoryStorage() : new FileSystemStorage(Path.Combine(LogRoot, "Output"));

            // SearchAPI最多只能读200条，因此按200分组
            using (StreamWriter error = new StreamWriter(Path.Combine(LogRoot, "error.txt"), false, Encoding.UTF8)) {
                if (computeDiff) {
                    IDictionary<int, string> list = input.Get<IDictionary<int, string>>();
                    try {
                        Utility.Partition(list, 200).AsParallel().ForAll(
                            part => GoSearchApi(part.Select(p => p.Key).ToArray(), error, output, list));
                    }
                    catch (AggregateException ex) {
                        Log.ErrorException("AggregateException occured", ex.InnerException ?? ex);
                    }

                    watch.Stop();
                    Log.Info("Work done using {0}min, found {1} entries", watch.Elapsed.Minutes, list.Count - notFound.Count);
                }
                else {
                    ICollection<int> list = input.Get<ICollection<int>>();
                    Utility.Partition(list, 200).AsParallel().ForAll(
                        part => GoSearchApi(part, error, output, null));

                    watch.Stop();
                    Log.Info("Work done using {0}min, found {1} entries", watch.Elapsed.Minutes, list.Count - notFound.Count);
                }
            }

            Log.Info("Not found: {0}", String.Join(",", notFound.ToArray()));
            output.Add("NotFound", notFound);
            return output;
        }

        private void GoSearchApi(int[] list, StreamWriter error, IStorage output, IDictionary<int, string> compareBase) {
            App[] apps = GetAppsFromSearchApi(list, error);
            if (computeDiff && compareBase != null) {
                List<App> filtered = new List<App>();
                foreach (App app in apps) {
                    if (app.Brief.Hash != compareBase[app.Id]) {
                        filtered.Add(app);
                        Log.Debug("{0} differs from origin", app.Id);
                    }
                }
                Log.Info("Diffs {0} entries out of {1} using hash", filtered.Count, apps.Length);
                apps = filtered.ToArray();
            }
            output.Add(apps);
        }

        private App[] GetAppsFromSearchApi(int[] list, StreamWriter error) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            string url = String.Format("http://itunes.apple.com/lookup?country=cn&&lang=zh_cn&id={0}", String.Join(",", list));

            for (int i = 0; i < 5; i++) {
                using (WebClient client = new WebClient()) {
                    client.Encoding = Encoding.UTF8;
                    try {
                        string json = client.DownloadString(url);
                        App[] apps = Utility.ParseSearchApiResponse(json);

                        IEnumerable<int> diff = list.Except(apps.Select(a => a.Id));
                        foreach (int id in diff) {
                            notFound.Add(id);
                            Log.Trace("{0} not found", id);
                        }

                        watch.Stop();
                        Log.Info("Require: {0:000}    Found: {1:000}    Miss: {2:00}    Time: {3}ms", list.Length, apps.Length, diff.Count(), watch.ElapsedMilliseconds);

                        return apps;
                    }
                    catch (WebException ex) {
                        Log.WarnException(url, ex);
                    }
                    catch (ArgumentNullException ex) {
                        Log.WarnException(url, ex);
                    }
                }
            }

            Log.Error(url + " finally fail");
            lock (error) {
                error.WriteLine(url);
            }

            return new App[0];
        }
    }
}
