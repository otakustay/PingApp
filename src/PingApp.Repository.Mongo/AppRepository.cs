using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.Mongo {
    public sealed class AppRepository : IAppRepository {
        private readonly MongoCollection<App> apps;

        private readonly MongoCollection<RevokedApp> revokedApps;

        private readonly MongoCollection<AppUpdate> appUpdates;


        public AppRepository(MongoCollection<App> apps,
            MongoCollection<RevokedApp> revokedApps, MongoCollection<AppUpdate> appUpdates) {
            this.apps = apps;
            this.revokedApps = revokedApps;
            this.appUpdates = appUpdates;
        }

        public App Retrieve(int id) {
            App app = apps.AsQueryable<App>().First(a => a.Id == id);
            return app;
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            App[] result = apps.Find(Query.In("_id", BsonArray.Create(required))).ToArray();
            return result;
        }

        public ICollection<AppBrief> RetrieveBriefs(IEnumerable<int> required) {
            AppBrief[] result = apps.Find(Query.In("_id", BsonArray.Create(required)))
                .SetFields("brief")
                .Select(a => a.Brief)
                .ToArray();
            return result;
        }

        public DeveloperAppsQuery RetrieveByDeveloper(DeveloperAppsQuery query) {
            if (query.Developer == null) {
                throw new ArgumentNullException("query.Developer");
            }

            IMongoQuery mongoQuery = Query.EQ("brief.developer._id", query.Developer.Id);
            AppBrief[] result = apps.Find(mongoQuery)
                .SetFields("brief")
                .SetSkip(query.SkipSize)
                .SetLimit(query.TakeSize)
                .Select(a => a.Brief)
                .ToArray();
            query.Fill(result);

            // 把参数补齐
            if (result.Length > 0) {
                query.Developer.Name = result[0].Developer.Name;
                query.Developer.ViewUrl = result[0].Developer.ViewUrl;
            }
            else {
                query.Developer.Name = String.Empty;
                query.Developer.ViewUrl = String.Empty;
            }

            return query;
        }

        public AppListQuery Search(AppListQuery query) {
            List<IMongoQuery> mongoQueries = new List<IMongoQuery>();

            if (query.DeviceType != DeviceType.NotProvided) {
                mongoQueries.Add(
                    Query.Or(
                        Query.EQ("brief.deviceType", query.DeviceType),
                        Query.EQ("brief.deviceType", DeviceType.Universal)
                    )
                );
            }

            if (!String.IsNullOrEmpty(query.Category)) {
                Category category = Category.Get(query.Category);
                if (category != null) {
                    mongoQueries.Add(Query.EQ("brief.primaryCategory", category.Id));
                }
            }

            if (query.PriceMode == PriceMode.Free) {
                mongoQueries.Add(Query.EQ("brief.price", 0));
            }
            else if (query.PriceMode == PriceMode.Paid) {
                mongoQueries.Add(Query.GT("brief.price", 0));
            }

            if (query.UpdateType.HasValue) {
                mongoQueries.Add(Query.EQ("brief.lastValidUpdate.type", query.UpdateType.Value));
            }

            MongoCursor<App> baseCursor = mongoQueries.Count == 0 ?
                apps.FindAll() : 
                apps.Find(Query.And(mongoQueries.ToArray()));
            AppBrief[] result = baseCursor
                .SetFields("brief")
                .SetSkip(query.SkipSize)
                .SetLimit(query.TakeSize)
                .Select(a => a.Brief)
                .ToArray();
            query.Fill(result);

            return query;
        }

        public void Save(App app) {
            AppUpdate updateForNew = new AppUpdate() {
                App = app.Id,
                Time = app.Brief.ReleaseDate.Date,
                Type = AppUpdateType.New,
                NewValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol
            };
            appUpdates.Save(updateForNew);
            AppUpdate updateForAdd = new AppUpdate() {
                App = app.Id,
                Time = DateTime.Now,
                Type = AppUpdateType.AddToPing,
                NewValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol
            };
            appUpdates.Save(updateForAdd);

            app.Brief.LastValidUpdate = updateForAdd;
            apps.Save(app);
        }

        public void Update(App app) {
            apps.Save(app);
        }

        public ICollection<App> Retrieve(int offset, int limit) {
            App[] result = apps.FindAll()
                .SetSkip(offset)
                .SetLimit(limit)
                .ToArray();
            return result;
        }

        public RevokedApp Revoke(App app) {
            // 添加下架信息
            AppUpdate update = new AppUpdate() {
                App = app.Id,
                Type = AppUpdateType.Revoke,
                OldValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol,
                Time = DateTime.Now
            };
            appUpdates.Save(update);

            // 从在售应用中移除
            apps.Remove(Query.EQ("_id", app.Id), RemoveFlags.Single);

            // 添加到下架应用中
            RevokedApp revoked = new RevokedApp(app);
            revokedApps.Save(revoked);

            return revoked;
        }

        public ICollection<RevokedApp> RetrieveRevoked(int offset, int limit) {
            RevokedApp[] result = revokedApps.FindAll()
                .SetSkip(offset)
                .SetLimit(limit)
                .ToArray();
            return result;
        }

        public void Resurrect(App app) {
            // 添加重新上架信息
            AppUpdate update = new AppUpdate() {
                App = app.Id,
                NewValue = app.Brief.Version + ", " + app.Brief.PriceWithSymbol,
                Time = DateTime.Now,
                Type = AppUpdateType.Resurrect
            };
            appUpdates.Save(update);

            // 从下架应用中移除
            revokedApps.Remove(Query.EQ("_id", app.Id));

            // 重新加入到应用集合中，并标记最后更新
            app.Brief.LastValidUpdate = update;
            apps.Save(app);
        }
    }
}
