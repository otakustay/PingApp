using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class AppUpdateRepository : IAppUpdateRepository {
        private readonly MongoCollection<AppUpdate> appUpdates;

        public AppUpdateRepository(MongoCollection<AppUpdate> appUpdates) {
            this.appUpdates = appUpdates;
        }

        public void Save(AppUpdate update) {
            appUpdates.Save(update);
        }
    }
}
