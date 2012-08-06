using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.MySql {
    public class AppRepository : IAppRepository {
        public App Retrieve(int app) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public ICollection<AppBrief> RetrieveBriefs(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public Quries.DeveloperAppsQuery RetrieveByDeveloper(DeveloperAppsQuery query) {
            throw new NotImplementedException();
        }

        public Quries.AppListQuery Search(Quries.AppListQuery query) {
            throw new NotImplementedException();
        }

        public void Save(App app) {
            throw new NotImplementedException();
        }

        public void Update(App app) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(int offset, int limit) {
            throw new NotImplementedException();
        }

        public RevokedApp Revoke(App app) {
            throw new NotImplementedException();
        }

        public ICollection<RevokedApp> RetrieveRevoked(int offset, int limit) {
            throw new NotImplementedException();
        }

        public void Resurrect(App resurrected) {
            throw new NotImplementedException();
        }
    }
}
