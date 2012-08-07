using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.MySql {
    public class AppUpdateRepository : IAppUpdateRepository, IDisposable {
        private readonly MySqlConnection connection;

        public AppUpdateRepository(MySqlConnection connection) {
            this.connection = connection;
        }

        public AppUpdateQuery RetrieveByApp(AppUpdateQuery query) {
            throw new NotImplementedException();
        }

        public void Save(AppUpdate update) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            connection.Dispose();
        }
    }
}
