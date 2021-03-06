﻿using System;
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
using PingApp.Infrastructure.Lucene;

namespace PingApp.Infrastructure.Default {
    sealed class LuceneIndexer : IAppIndexer {
        private static readonly ILogger logger = ProgramSettings.GetLogger<LuceneIndexer>();

        private readonly IndexWriter writer;

        private readonly ProgramSettings settings;

        private Queue<App> addQueue = new Queue<App>();

        private Queue<App> updateQueue = new Queue<App>();

        private Queue<App> deleteQueue = new Queue<App>();

        public LuceneIndexer(bool rebuild, ProgramSettings settings) {
            this.settings = settings;

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

        public void DeleteApp(App app) {
            lock (deleteQueue) {
                deleteQueue.Enqueue(app);
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

            lock (updateQueue) {
                while (updateQueue.Count > 0) {
                    App app = updateQueue.Dequeue();
                    Term term = CreateTerm(app);

                    writer.DeleteDocuments(term);

                    logger.Trace("Deleted index for app {0}-{1}", app.Id, app.Brief.Name);
                }
            }
        }

        public void Dispose() {
            try {
                Flush();

                Stopwatch watch = new Stopwatch();
                watch.Start();

                writer.Optimize();

                watch.Stop();
                logger.Info("Optimized index using {0}", watch.Elapsed);
            }
            finally {
                writer.Dispose();
            }

            logger.Info("Disposed lucene indexer");
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

            doc.Add(id);
            doc.Add(name);
            doc.Add(description);
            doc.Add(developerName);
            doc.Add(category);
            doc.Add(deviceType);
            doc.Add(languagePriority);

            // 排序字段
            Field nameForSort = new Field("NameForSort", app.Brief.Name, Field.Store.NO, Field.Index.NOT_ANALYZED);
            NumericField lastValidUpdateTimee = new NumericField("LastValidUpdateTime", Field.Store.NO, true);
            lastValidUpdateTimee.SetLongValue(app.Brief.LastValidUpdate.Time.Ticks);
            NumericField price = new NumericField("Price", Field.Store.NO, true);
            price.SetFloatValue(app.Brief.Price);
            NumericField ratingCount = new NumericField("Rating", Field.Store.NO, true);
            ratingCount.SetIntValue(app.Brief.AverageUserRatingForCurrentVersion.HasValue ? (int)app.Brief.AverageUserRatingForCurrentVersion : 0);

            doc.Add(nameForSort);
            doc.Add(lastValidUpdateTimee);
            doc.Add(price);
            doc.Add(ratingCount);

            return doc;
        }

        private Term CreateTerm(App app) {
            Term term = new Term("Id", app.Id.ToString());
            return term;
        }
    }
}
