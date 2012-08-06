﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.MySql {
    public class AppRepository : IAppRepository, IDisposable {
        private readonly MySqlConnection connection;

        public AppRepository(MySqlConnection connection) {
            this.connection = connection;
        }

        public App Retrieve(int app) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public ICollection<AppBrief> RetrieveBriefs(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public DeveloperAppsQuery RetrieveByDeveloper(DeveloperAppsQuery query) {
            throw new NotImplementedException();
        }

        public AppListQuery Search(Quries.AppListQuery query) {
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

        public void Dispose() {
            connection.Dispose();
        }
    }
}
