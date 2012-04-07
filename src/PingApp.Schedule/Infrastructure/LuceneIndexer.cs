using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NLog;
using PingApp.Entity;
using PingApp.Utility.Lucene;
using SimpleLucene;
using SimpleLucene.Impl;

namespace PingApp.Schedule.Infrastructure {
    sealed class LuceneIndexer : IDisposable {
        private static readonly AppIndexDefinition definition = new AppIndexDefinition();

        private readonly ProgramSettings settings;

        private readonly Logger logger;

        private readonly IndexWriter writer;

        private Queue<App> addQueue = new Queue<App>();

        private Queue<App> updateQueue = new Queue<App>();

        public LuceneIndexer(bool rebuild, ProgramSettings settings, Logger logger) {
            this.settings = settings;
            this.logger = logger;

            writer = CreateIndexWriter(rebuild);
        }

        public void AddApp(App app) {
            lock (addQueue) {
                addQueue.Enqueue(app);
            }
        }

        public void UpdateApp(App app) {
            lock (updateQueue) {
                updateQueue.Enqueue(app);
            }
        }

        public void Flush() {
            lock (addQueue) {
                while (addQueue.Count > 0) {
                    App app = addQueue.Dequeue();
                    Document document = CreateDocument(app);

                    writer.AddDocument(document);

                    logger.Trace("Added index for app {0}-{1}", app.Id, app.Brief.Name);
                }
            }

            lock (updateQueue) {
                while (updateQueue.Count > 0) {
                    App app = updateQueue.Dequeue();
                    Document document = CreateDocument(app);
                    Term term = CreateTerm(app);

                    writer.UpdateDocument(term, document);

                    logger.Trace("Updated index for app {0}-{1}", app.Id, app.Brief.Name);
                }
            }
        }

        public void Dispose() {
            try {
                Flush();
                writer.Optimize();
            }
            finally {
                writer.Dispose();
            }
        }

        private IndexWriter CreateIndexWriter(bool rebuild) {
            IndexWriter writer = new IndexWriter(
                FSDirectory.Open(new DirectoryInfo(settings.LucentDirectory)),
                new PanGuAnalyzer(),
                rebuild,
                IndexWriter.MaxFieldLength.UNLIMITED
            );

            return writer;
        }

        private Document CreateDocument(App app) {
            Document doc = new Document();
            Field id = new Field("Id", app.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
            Field name = new Field("Name", app.Brief.Name, Field.Store.NO, Field.Index.ANALYZED);
            Field description = new Field("Description", app.Description ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED);
            Field developerName = new Field("DeveloperName", app.Brief.Developer.Name ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED);
            Field category = new Field("Category", String.Join(" ", app.Categories.Select(c => c.Id)), Field.Store.NO, Field.Index.ANALYZED);
            NumericField deviceType = new NumericField("DeviceType", Field.Store.NO, true);
            deviceType.SetIntValue((int)app.Brief.DeviceType);
            NumericField languagePriority = new NumericField("LanguagePriority", Field.Store.NO, true);
            languagePriority.SetIntValue(app.Brief.LanguagePriority);

            doc.AddField(id);
            doc.AddField(name);
            doc.AddField(description);
            doc.AddField(developerName);
            doc.AddField(category);
            doc.AddField(deviceType);
            doc.AddField(languagePriority);

            // 排序字段
            Field nameForSort = new Field("NameForSort", app.Brief.Name, Field.Store.NO, Field.Index.NOT_ANALYZED);
            NumericField lastValidUpdateTimee = new NumericField("LastValidUpdateTime", Field.Store.NO, true);
            lastValidUpdateTimee.SetLongValue(app.Brief.LastValidUpdate.Time.Ticks);
            NumericField price = new NumericField("Price", Field.Store.NO, true);
            price.SetFloatValue(app.Brief.Price);
            NumericField ratingCount = new NumericField("Rating", Field.Store.NO, true);
            ratingCount.SetIntValue(app.Brief.AverageUserRatingForCurrentVersion.HasValue ? (int)app.Brief.AverageUserRatingForCurrentVersion : 0);

            doc.AddField(nameForSort);
            doc.AddField(lastValidUpdateTimee);
            doc.AddField(price);
            doc.AddField(ratingCount);

            return doc;
        }

        private Term CreateTerm(App app) {
            Term term = new Term("Id", app.Id.ToString());
            return term;
        }
    }
}
