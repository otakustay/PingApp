using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NLog;
using PingApp.Entity;
using PingApp.Repository;
using PingApp.Schedule.Infrastructure;

namespace PingApp.Schedule.Task {
    sealed class InitializeTask : TaskBase {
        private readonly CatalogParser catalogParser;

        private readonly AppParser appParser;

        private readonly LuceneIndexer indexer;

        private readonly RepositoryEmitter repository;

        public InitializeTask(CatalogParser catalogParser, AppParser appParser,
            LuceneIndexer indexer, RepositoryEmitter repository, Logger logger) : base(logger) {
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

            logger.Info("Start parse catalogs");
            ISet<int> identities = catalogParser.CollectApps();
            logger.Info("Catalogs parsed");

            logger.Info("Start find and save apps");
            // Search API一次最多能传200个id，所以设定以200为一个区块
            int appCount = Partition(identities, 200).AsParallel().Sum(p => FindAndSaveApps(p));
            logger.Info("Saved {0} apps", appCount);

            watch.Stop();
            logger.Info("Finished task using {0}", watch.Elapsed);
        }

        private int FindAndSaveApps(IEnumerable<int> partition) {
            ICollection<App> apps = appParser.RetrieveApps(partition);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (App app in apps) {
                AppUpdate updateForNew = new AppUpdate() {
                    App = app.Id,
                    Time = app.Brief.ReleaseDate.Date,
                    Type = AppUpdateType.New,
                    OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol
                };
                repository.AppUpdate.Save(updateForNew);
                AppUpdate updateForAdd = new AppUpdate() {
                    App = app.Id,
                    Time = DateTime.Now,
                    Type = AppUpdateType.AddToNote,
                    OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol
                };
                repository.AppUpdate.Save(updateForAdd);

                app.Brief.LastValidUpdate = updateForAdd;
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

            return apps.Count();
        }



        private static IEnumerable<IEnumerable<T>> Partition<T>(IEnumerable<T> list, int size) {
            List<T> output = new List<T>(size);
            foreach (T item in list) {
                output.Add(item);
                if (output.Count % size == 0) {
                    yield return output;
                    output = new List<T>(size);
                }
            }
            if (output.Count > 0) {
                yield return output;
            }
        }

        public override void Dispose() {
            indexer.Dispose();
        }
    }
}
