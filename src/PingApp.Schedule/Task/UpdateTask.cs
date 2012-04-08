using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using PingApp.Entity;
using PingApp.Repository;
using PingApp.Schedule.Infrastructure;
using Tasks = System.Threading.Tasks;

namespace PingApp.Schedule.Task {
    sealed class UpdateTask : TaskBase {
        private readonly AppParser appParser;

        private readonly LuceneIndexer indexer;

        private readonly UpdateNotifier notifier;

        private readonly RepositoryEmitter repository;

        private readonly ProgramSettings settings;

        private int offset = 0;

        private readonly int limit;

        public UpdateTask(AppParser appParser, LuceneIndexer indexer, UpdateNotifier notifier,
            RepositoryEmitter repository, ProgramSettings settings, Logger logger)
            : base(logger) {
            this.appParser = appParser;
            this.indexer = indexer;
            this.notifier = notifier;
            this.repository = repository;
            this.settings = settings;

            limit = settings.BatchSize / 200 * 200; // 因为Search API是200一批，找个最接近的200的倍数，以免浪费
        }

        public override void Run(string[] args) {
            /*
             * 1. 如果有参数，则每个参数是一个应用id，没有则取数据库中全量数据
             * 2. 以200为一份进行分隔
             * 3. 每一份从Search API上获取对应的数据，进行检查：
             *    3.1. 如果Search API上有信息，检查更新：
             *         3.1.1. 应用前后相等，不作更新
             *         3.1.2. 有重要更新：
             *                3.1.2.1. 插入更新数据，设定应用最后更新信息
             *                3.1.2.2. 通知用户相关更新
             *         3.1.3. 更新应用信息
             *    3.2. 如果Search API上没信息，视为下架：
             *         3.2.1. 如果应用已经下架，不做处理
             *         3.2.2. 添加下架的更新信息，设定应用最后更新为下架
             *    3.3. 更新Lucene索引
             */
            Stopwatch watch = new Stopwatch();

            logger.Info("Start update task");
            watch.Start();

            TaskFactory factory = new TaskFactory();
            List<Tasks.Task> tasks = new List<Tasks.Task>();

            if (args.Length > 0) {
                // 按200一组取数据库的数据判定更新
                foreach (IEnumerable<int> identities in args.Partition(200)) {
                    Stopwatch stepWatch = new Stopwatch();
                    stepWatch.Start();

                    ICollection<App> apps = repository.App.Retrieve(identities);

                    stepWatch.Stop();
                    logger.Trace("Retrieved {0} apps from database using {1}ms", apps.Count, stepWatch.ElapsedMilliseconds);

                    Tasks.Task task = factory.StartNew(CheckUpdates, apps);
                    tasks.Add(task);
                }
            }
            else {
                // 从数据库分批取
                for (int i = 0; i < Environment.ProcessorCount; i++) {
                    Tasks.Task task = factory.StartNew(RetrieveAndUpdate);
                    tasks.Add(task);
                }
            }
            Tasks.Task.WaitAll(tasks.ToArray());

            watch.Stop();
            logger.Info("Finished task using {0}", watch.Elapsed);
        }

        private void RetrieveAndUpdate() {
            while (true) {
                ICollection<App> apps;

                lock (repository) {
                    if (offset < 0) {
                        return;
                    }

                    logger.Trace("Retrieve apps in range {0}-{1}", offset, offset + limit);
                    Stopwatch stepWatch = new Stopwatch();
                    stepWatch.Start();

                    apps = repository.App.Retrieve(offset, limit);

                    if (apps.Count < limit) {
                        offset = -1;
                        return;
                    }

                    offset += limit;

                    stepWatch.Stop();
                    logger.Debug("Retrieved {0} apps from database using {1}ms", apps.Count, stepWatch.ElapsedMilliseconds);
                }

                foreach (IEnumerable<App> partition in apps.Partition(200)) {
                    CheckUpdates(partition);
                }
            }
        }

        private void CheckUpdates(object input) {
            IEnumerable<App> apps = input as IEnumerable<App>;
            IEnumerable<int> identities = apps.Select(a => a.Id);
            ICollection<App> retrievedApps = appParser.RetrieveApps(identities);

            if (retrievedApps == null) {
                return;
            }

            Dictionary<int, App> updated = retrievedApps.ToDictionary(a => a.Id);

            foreach (App app in apps) {
                if (updated.ContainsKey(app.Id)) {
                    // 检查是否有更新
                    CheckUpdateForApp(app, updated[app.Id]);
                }
                else {
                    // 数据库中有，但Search API上没有，定义为下架
                    CheckOffSellForApp(app);
                }
            }

            indexer.Flush();
        }

        public override void Dispose() {
            try {
                logger.Info("Disposing lucene indexer");
                indexer.Dispose();
                logger.Info("Disposed lucene indexer");
            }
            finally {
                logger.Info("Disposing update notifier");
                notifier.Dispose();
                logger.Info("Disposed update notifier");
            }
        }

        private void CheckUpdateForApp(App original, App updated) {
            if (original.Equals(updated)) {
                return;
            }

            // 检查/添加更新信息
            ICollection<AppUpdate> validUpdates = original.Brief.CheckForUpdate(updated.Brief);
            foreach (AppUpdate update in validUpdates) {
                repository.AppUpdate.Save(update);

                logger.Trace(
                    "Added update of type {0} for app {1}-{2}",
                    update.Type, original.Id, original.Brief.Name
                );

                if (AppUpdate.IsValidUpdate(update.Type)) {
                    updated.Brief.LastValidUpdate = update;
                }

                // 通知用户
                notifier.ProcessUpdate(updated, update);
            }

            // 更新应用
            original.UpdateFrom(updated);
            repository.App.Update(original);

            // 更新索引
            indexer.UpdateApp(original);

            logger.Trace("Updated app {0}-{1}", original.Id, original.Brief.Name);
        }

        private void CheckOffSellForApp(App app) {
            // 原本已经是下架状态，不作额外更新
            if (!app.Brief.IsActive) {
                return;
            }

            // 添加下架信息
            AppUpdate offUpdate = new AppUpdate() {
                App = app.Id,
                Type = AppUpdateType.Off,
                OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol,
                Time = DateTime.Now
            };
            repository.AppUpdate.Save(offUpdate);

            logger.Trace(
                "Added update of type {0} for app {1}-{2}",
                offUpdate.Type, app.Id, app.Brief.Name
            );

            // 更新应用
            app.Brief.IsActive = false;
            repository.App.Update(app);

            logger.Trace("Set app {0}-{1} to be off sold", app.Id, app.Brief.Name);

            // 下架的应用不需要删除索引，也不需要更新索引
        }
    }
}
