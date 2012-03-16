using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PingApp.Entity;
using PingApp.Utility.Lucene;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Newtonsoft.Json.Linq;
using SimpleLucene;
using SimpleLucene.Impl;
using PingApp.Schedule.Storage;

namespace PingApp.Schedule.Task {
    class IndexTask : TaskNode {
        private readonly bool incremental;

        public IndexTask(bool incremental) {
            this.incremental = incremental;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start lucene index using --incremental={0}", incremental);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            if (incremental) {
                UpdateIndex(input);
            }
            else {
                CreateIndex(input);
            }

            IStorage output = new MemoryStorage();
            IEnumerable<AppUpdate> updates = input.Get<IEnumerable<AppUpdate>>("Updates");
            if (updates != null) {
                output.Add(updates);
            }

            watch.Stop();
            Log.Info("Work done using {0}ms", watch.ElapsedMilliseconds);

            return output;
        }

        private void CreateIndex(IStorage input) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            IIndexWriter writer = CreateIndexWriter();
            AppIndexDefinition definition = new AppIndexDefinition();
            using (IndexService service = new IndexService(writer)) {
                while (input.HasMore) {
                    ICollection<App> apps = input.Get<ICollection<App>>();
                    service.IndexEntities(apps, definition);

                    Log.Info("{0} apps processed", apps.Count);
                }
            }

            watch.Stop();
            Log.Info("index created using {0}ms", watch.ElapsedMilliseconds);
        }

        private void UpdateIndex(IStorage input) {
            Log.Info("Start update lucene index");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            IIndexWriter writer = CreateIndexWriter();
            AppIndexDefinition definition = new AppIndexDefinition();
            int count = 0;
            ICollection<App> added = input.Get<ICollection<App>>("New");
            ICollection<App> updated = input.Get<ICollection<App>>("Updated");
            using (IndexService service = new IndexService(writer)) {
                while (input.HasMore) {
                    ICollection<App> apps = input.Get<ICollection<App>>();
                    // 默认数据里全是新的
                    foreach (App app in apps) {
                        service.IndexEntities(apps, definition);
                        Log.Debug("Added {0} : {1}", app.Id, app.Brief.Name);
                    }
                    count += apps.Count;
                }
                // 处理New和Updated
                foreach (App app in added) {
                    service.IndexEntity(app, definition);
                    Log.Debug("Added {0} : {1}", app.Id, app.Brief.Name);
                }
                // IndexEntities只有Add没有Update，不好用
                foreach (App app in updated) {
                    service.IndexEntity(app, definition);
                    Log.Debug("Updated {0} : {1}", app.Id, app.Brief.Name);
                }
            }

            watch.Stop();
            Log.Info(
                "Update index done using {0}ms, add {1} entries, update {2} entries", 
                watch.ElapsedMilliseconds, count + added.Count, updated.Count
            );
        }

        private IIndexWriter CreateIndexWriter() {
            IIndexWriter writer = new DirectoryIndexWriter(
                new DirectoryInfo(ConfigurationManager.AppSettings["LuceneIndexDirectory"]),
                new PanGuAnalyzer(),
                !incremental
            );
            writer.IndexOptions.OptimizeIndex = true;

            return writer;
        }
    }
}
