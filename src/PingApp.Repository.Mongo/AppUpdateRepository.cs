using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.Mongo {
    public sealed class AppUpdateRepository : IAppUpdateRepository {
        private readonly MongoCollection<AppUpdate> appUpdates;

        public AppUpdateRepository(MongoCollection<AppUpdate> appUpdates) {
            this.appUpdates = appUpdates;
        }

        public AppUpdateQuery RetrieveByApp(AppUpdateQuery query) {
            List<IMongoQuery> mongoQueries = new List<IMongoQuery>();

            mongoQueries.Add(Query.EQ("app", query.App));
            mongoQueries.Add(Query.LTE("time", query.LatestTime));
            if (query.EarliestTime.HasValue) {
                mongoQueries.Add(Query.GTE("time", query.EarliestTime));
            }

            AppUpdate[] result = appUpdates.Find(Query.And(mongoQueries.ToArray())).ToArray();
            query.Fill(result);

            return query;
        }

        public void Save(AppUpdate update) {
            appUpdates.Save(update);
        }
    }
}
