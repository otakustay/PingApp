using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.MySql {
    public class AppTrackRepository : IAppTrackRepository, IDisposable {
        private readonly MySqlConnection connection;

        public AppTrackRepository(MySqlConnection connection) {
            this.connection = connection;
        }
        public void Save(AppTrack track) {
            throw new NotImplementedException();
        }

        public void Update(AppTrack track) {
            throw new NotImplementedException();
        }

        public void Remove(Guid id) {
            throw new NotImplementedException();
        }

        public AppTrack Retrieve(Guid user, int app) {
            throw new NotImplementedException();
        }

        public AppTrackQuery Retrieve(AppTrackQuery query) {
            throw new NotImplementedException();
        }

        public ICollection<AppTrack> RetrieveByApp(int app) {
            throw new NotImplementedException();
        }

        public int ResetReadStatusByApp(int app) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            connection.Dispose();
        }
    }
}
