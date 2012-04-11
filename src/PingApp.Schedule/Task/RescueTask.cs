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
    sealed class RescueTask : TaskBase {
        private readonly AppParser appParser;

        private readonly LuceneIndexer indexer;

        private readonly RepositoryEmitter repository;

        private int offset = 0;

        private readonly int limit;

        public RescueTask(AppParser appParser, LuceneIndexer indexer,
            RepositoryEmitter repository, ProgramSettings settings, Logger logger)
            : base(logger) {
            this.appParser = appParser;
            this.indexer = indexer;
            this.repository = repository;

            limit = settings.BatchSize / 200 * 200; // 因为Search API是200一批，找个最接近的200的倍数，以免浪费
        }

        public override void Run(string[] args) {
            /*
             * 1. 获取RevokedApp，以200为一组
             * 2. 从Search API上找数据，找到的可复活
             * 3. 对于找到的数据
             *    3.1. 添加一条Resurrect类型的更新信息
             *    3.2. 从RevokeApp中移除
             *    3.3. 新增到App中
             *    
             * XXX: 一次Rescue任务不会复活太多的应用，从详细日志里能看到数量，不实现计数功能了
             */
            Stopwatch watch = new Stopwatch();

            logger.Info("Start rescue task");
            watch.Start();

            // 从数据库分批取
            TaskFactory factory = new TaskFactory();
            List<Tasks.Task> tasks = new List<Tasks.Task>();
            for (int i = 0; i < Environment.ProcessorCount; i++) {
                Tasks.Task task = factory.StartNew(RetrieveAndRescue);
                tasks.Add(task);
            }
            Tasks.Task.WaitAll(tasks.ToArray());

            watch.Stop();
            logger.Info("Finished task using {0}", watch.Elapsed);
        }

        public override void Dispose() {
            base.Dispose();
            indexer.Dispose();
        }

        private void RetrieveAndRescue() {
            while (true) {
                ICollection<RevokedApp> apps;

                lock (repository) {
                    if (offset < 0) {
                        return;
                    }

                    logger.Trace("Retrieve apps in range {0}-{1}", offset, offset + limit);
                    Stopwatch stepWatch = new Stopwatch();
                    stepWatch.Start();

                    apps = repository.App.RetrieveRevoked(offset, limit);

                    if (apps.Count < limit) {
                        offset = -1;
                    }
                    else {
                        offset += limit;
                    }

                    stepWatch.Stop();
                    logger.Debug("Retrieved {0} apps from database using {1}ms", apps.Count, stepWatch.ElapsedMilliseconds);
                }

                foreach (IEnumerable<RevokedApp> partition in apps.Partition(200)) {
                    TryRescue(partition);
                }
            }
        }

        private void TryRescue(IEnumerable<RevokedApp> apps) {
            int[] identities = apps.Select(a => a.Id).ToArray();
            ICollection<App> retrievedApps = appParser.RetrieveApps(identities);

            if (retrievedApps == null) {
                return;
            }

            foreach (App resurrected in retrievedApps) {
                repository.App.Resurrect(resurrected);

                indexer.AddApp(resurrected);

                logger.Trace("Resurrected app {0}-{1}", resurrected.Id, resurrected.Brief.Name);
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            indexer.Flush();

            watch.Stop();
            logger.Debug("Indexed {0} apps using {1}ms", retrievedApps.Count, watch.ElapsedMilliseconds);
        }
    }
}
