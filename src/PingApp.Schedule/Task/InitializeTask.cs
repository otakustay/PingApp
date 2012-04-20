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
    sealed class InitializeTask : TaskBase {
        private readonly ICatalogParser catalogParser;

        private readonly IAppParser appParser;

        private readonly IAppIndexer indexer;

        private readonly RepositoryEmitter repository;

        public InitializeTask(ICatalogParser catalogParser, IAppParser appParser,
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
             * 2. 依次从Search API查找具体内容
             * 3. 每个新的App添加2个Update（New和AddToNote），App的LastValidUpdate是New
             * 4. 将App和Update放入数据库中
             * 5. 添加到Lucene索引
             */
            Stopwatch watch = new Stopwatch();

            logger.Info("Start initialize task");
            watch.Start();

            ISet<int> identities = catalogParser.CollectApps();

            logger.Info("Start find and save apps");
            // Search API一次最多能传200个id，所以设定以200为一个区块
            int appCount = identities.Partition(200).AsParallel()
                .WithDegreeOfParallelism(settings.ParallelDegree)
                .Sum(p => FindAndSaveApps(p));
            logger.Info("Saved {0} apps", appCount);

            watch.Stop();
            logger.Info("Finished task using {0}", watch.Elapsed);
        }

        public override void Dispose() {
            base.Dispose();
            indexer.Dispose();
        }

        private int FindAndSaveApps(ICollection<int> partition) {
            ICollection<App> apps = appParser.RetrieveApps(partition);

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
    }
}
