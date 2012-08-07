using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using PingApp.Entity;
using PingApp.Infrastructure;
using PingApp.Repository;
using Tasks = System.Threading.Tasks;

namespace PingApp.Schedule.Task {
    sealed class UpdateTask : TaskBase {
        private static readonly ILogger logger = ProgramSettings.GetLogger<UpdateTask>();

        private readonly IAppParser appParser;

        private readonly IAppIndexer indexer;

        private readonly IUpdateNotifier notifier;

        private readonly RepositoryEmitter repository;

        private int offset = 0;

        private readonly int limit;

        private readonly List<App> revokedApps = new List<App>();

        public UpdateTask(IAppParser appParser, IAppIndexer indexer, IUpdateNotifier notifier,
            RepositoryEmitter repository, ProgramSettings settings)
            : base(settings) {
            this.appParser = appParser;
            this.indexer = indexer;
            this.notifier = notifier;
            this.repository = repository;

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
             *    3.2. 如果Search API上没信息，视为下架，添加到下架应用的集合中，等待后续处理
             *    3.3. 更新Lucene索引
             * 4. 处理下架的应用，添加下架的更新信息，放入下架应用中，并删除其Lucene索引
             */
            Stopwatch watch = new Stopwatch();

            logger.Info("Start update task");
            watch.Start();

            TaskFactory factory = new TaskFactory();
            List<Tasks.Task<int>> tasks = new List<Tasks.Task<int>>();

            if (args.Length > 0) {
                // 按200一组取数据库的数据判定更新
                foreach (IEnumerable<int> identities in args.Partition(200)) {
                    Stopwatch stepWatch = new Stopwatch();
                    stepWatch.Start();

                    ICollection<App> apps = repository.App.Retrieve(identities);

                    stepWatch.Stop();
                    logger.Trace("Retrieved {0} apps from database using {1}ms", apps.Count, stepWatch.ElapsedMilliseconds);

                    Tasks.Task<int> task = factory.StartNew<int>(CheckUpdates, apps);
                    tasks.Add(task);
                }
            }
            else {
                // 从数据库分批取
                for (int i = 0; i < settings.ParallelDegree; i++) {
                    Tasks.Task<int> task = factory.StartNew<int>(RetrieveAndUpdate);
                    tasks.Add(task);
                }
            }
            Tasks.Task.WaitAll(tasks.ToArray());

            logger.Info("Updated {0} apps", tasks.Sum(t => t.Result));

            /* 
             * 全部数据更新完毕后，统一处理下架的应用
             * 此时是单线程环境，不需要处理并发
             * （理论上）也没有其他的进程在读取库，因此可以任意删除数据
             */
            RevokeApps();

            watch.Stop();
            logger.Info("Finished task using {0}", watch.Elapsed);
        }

        public override void Dispose() {
            base.Dispose();
            try {
                indexer.Dispose();
            }
            finally {
                notifier.Dispose();
            }
        }

        private int RetrieveAndUpdate() {
            while (true) {
                ICollection<App> apps;

                lock (repository) {
                    if (offset < 0) {
                        return 0;
                    }

                    logger.Trace("Retrieve apps in range {0}-{1}", offset, offset + limit);
                    Stopwatch stepWatch = new Stopwatch();
                    stepWatch.Start();

                    apps = repository.App.Retrieve(offset, limit);

                    if (apps.Count < limit) {
                        offset = -1;
                    }
                    else {
                        offset += limit;
                    }

                    stepWatch.Stop();
                    logger.Debug("Retrieved {0} apps from database using {1}ms", apps.Count, stepWatch.ElapsedMilliseconds);
                }

                foreach (IEnumerable<App> partition in apps.Partition(200)) {
                    CheckUpdates(partition);
                }
            }
        }

        private int CheckUpdates(object input) {
            IEnumerable<App> apps = input as IEnumerable<App>;
            int[] identities = apps.Select(a => a.Id).ToArray();
            ICollection<App> retrievedApps = appParser.RetrieveApps(identities);

            if (retrievedApps == null) {
                return 0;
            }

            Dictionary<int, App> updated = retrievedApps.ToDictionary(a => a.Id);
            int count = 0;

            foreach (App app in apps) {
                if (updated.ContainsKey(app.Id)) {
                    // 检查是否有更新
                    bool changed = CheckUpdateForApp(app, updated[app.Id]);
                    if (changed) {
                        count++;
                    }
                }
                else {
                    /*
                     * 数据库中有，但Search API上没有，定义为下架
                     * 由于正在不断获取数据，此时移除Collection中的内容，会导致数据获取的不准确性，遗漏数据
                     * 因此先将下架的应用放入集合中，等待后续统一处理
                     * NOTICE: 由于这个逻辑的存在，Update任务不能和其他任务一起跑，会引起并发冲突（基本所有任务都不能和其他的并行）
                     */
                    lock (revokedApps) {
                        revokedApps.Add(app);
                    }
                }
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            indexer.Flush();

            watch.Stop();
            logger.Debug("Indexed {0} apps using {1}ms", retrievedApps.Count, watch.ElapsedMilliseconds);

            return count;
        }

        private bool CheckUpdateForApp(App original, App updated) {
            if (original.Equals(updated)) {
                return false;
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

            return true;
        }

        private void RevokeApps() {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (App app in revokedApps) {
                // 添加下架信息
                AppUpdate update = new AppUpdate() {
                    App = app.Id,
                    Type = AppUpdateType.Revoke,
                    OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol,
                    Time = DateTime.Now
                };
                repository.AppUpdate.Save(update);

                // 从在售应用中移除
                repository.App.Delete(app.Id);

                // 添加到下架应用中
                RevokedApp revoked = new RevokedApp(app);
                revoked.RevokeTime = DateTime.Now;
                repository.App.SaveRevoked(revoked);

                // 移除全文索引
                indexer.DeleteApp(app);

                logger.Trace("Set app {0}-{1} to be revoked", app.Id, app.Brief.Name);
            }

            watch.Stop();
            logger.Info("Revoked {0} apps using {1}ms", revokedApps.Count, watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Stop();

            indexer.Flush();

            watch.Stop();
            logger.Info("Deleted index for {0} apps using {1}ms", revokedApps.Count, watch.ElapsedMilliseconds);
        }
    }
}
