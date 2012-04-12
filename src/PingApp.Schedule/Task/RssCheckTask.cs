using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;
using PingApp.Entity;
using PingApp.Repository;
using PingApp.Schedule.Infrastructure;

namespace PingApp.Schedule.Task {
    sealed class RssCheckTask : TaskBase {
        private const string FEED_URL = "http://itunes.apple.com/cn/rss/newapplications/limit=300/xml";

        private readonly WebDownload download;

        private readonly AppParser appParser;

        private readonly LuceneIndexer indexer;

        private readonly RepositoryEmitter repository;

        private readonly ProgramSettings settings;

        public RssCheckTask(WebDownload download, AppParser appParser, LuceneIndexer indexer,
            RepositoryEmitter repository, ProgramSettings settings, Logger logger)
            : base(settings, logger) {
            this.download = download;
            this.appParser = appParser;
            this.indexer = indexer;
            this.repository = repository;
        }

        public override void Run(string[] args) {
            /*
             * 1. 从RSS Feed里找到最新的100个应用
             * 2. 从数据库找出对应的数据，做差值得到未进库的新应用
             * 3. 把新应用保存到数据库
             * 4. 更新索引
             */
            Stopwatch watch = new Stopwatch();

            logger.Info("Start rss check task");
            watch.Start();

            ICollection<int> identities = RetrieveFromFeed();
            ICollection<App> exists = repository.App.Retrieve(identities);

            if (exists == null) {
                logger.Info("Failed to run task due to network problem");
                return;
            }

            int[] required = identities.Except(exists.Select(a => a.Id)).ToArray();

            logger.Trace("These apps are new: {0}", String.Join(",", required));

            ICollection<App> newApps = appParser.RetrieveApps(required);

            logger.Trace("Found these apps in search api: {0}", String.Join(",", newApps.Select(a => a.Id)));

            SaveApps(newApps);

            watch.Stop();
            logger.Info("Finished task using {0}", watch.Elapsed);
        }

        public override void Dispose() {
            base.Dispose();
            indexer.Dispose();
        }

        private void SaveApps(ICollection<App> apps) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (App app in apps) {
                repository.App.Save(app);
                indexer.AddApp(app);
            }

            watch.Stop();
            logger.Info("Saved {0} apps to database using {1}ms", apps.Count, watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();

            indexer.Flush();

            watch.Stop();
            logger.Info("Indexed {0} apps using {1}ms", apps.Count, watch.ElapsedMilliseconds);
        }

        private ICollection<int> RetrieveFromFeed() {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            XDocument document = download.AsXml(FEED_URL);
            ICollection<int> apps = document.Root.Descendants("{http://www.w3.org/2005/Atom}entry")
                .Select(d => d.Elements("{http://www.w3.org/2005/Atom}id").First().Value)
                .Select(Utility.FindIdFromUrl)
                .ToArray();

            watch.Stop();
            logger.Info("Found {0} apps from rss feed using {1}ms", apps.Count, watch.ElapsedMilliseconds);

            return apps;
        }
    }
}
