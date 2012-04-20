using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NLog;
using PingApp.Entity;
using PingApp.Infrastructure;
using PingApp.Repository;

namespace PingApp.Schedule.Task {
    sealed class CheckNewTask : TaskBase {
        private readonly ICatalogParser catalogParser;

        private readonly IAppParser appParser;

        private readonly IAppIndexer indexer;

        private readonly RepositoryEmitter repository;

        public CheckNewTask(ICatalogParser catalogParser, IAppParser appParser,
            IAppIndexer indexer, RepositoryEmitter repository, ProgramSettings settings, Logger logger)
            : base(settings, logger) {
            this.catalogParser = catalogParser;
            this.appParser = appParser;
            this.indexer = indexer;
            this.repository = repository;
        }

        public override void Run(string[] args) {
            /*
             * 1. 从目录取得所有应用的id
             * 2. 找出这些id里数据库中不存在的部分
             * 3. 从Search API获取应用信息
             * 4. 放入数据库中
             * 5. 更新索引
             */
            Stopwatch watch = new Stopwatch();

            logger.Info("Start check new apps task");
            watch.Start();

            ISet<int> identities = catalogParser.CollectApps();
            int newAppCount = identities.Partition(200).AsParallel()
                .WithDegreeOfParallelism(settings.ParallelDegree)
                .Sum(p => FindAndSaveNewApps(p));
            logger.Info("Saved {0} new apps", newAppCount);

            watch.Stop();
            logger.Info("Finished task using {0}", watch.Elapsed);
        }

        private int FindAndSaveNewApps(ICollection<int> partition) {
            ICollection<App> existingApps = repository.App.Retrieve(partition);
            ICollection<int> diffs = partition.Except(existingApps.Select(a => a.Id)).ToArray();

            logger.Debug("Found {0} apps that does not exists in database", diffs.Count);

            ICollection<App> apps = appParser.RetrieveApps(diffs);

            if (apps == null) {
                return 0;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (App app in apps) {
                repository.App.Save(app);

                indexer.AddApp(app);
            }

            watch.Stop();
            logger.Debug("Saved {0} apps using {1}ms", apps.Count, watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();

            indexer.Flush();

            watch.Stop();
            logger.Debug("Indexed {0} apps using {1}ms", apps.Count, watch.ElapsedMilliseconds);

            return apps.Count;
        }

        public override void Dispose() {
            base.Dispose();
            indexer.Dispose();
        }
    }
}
