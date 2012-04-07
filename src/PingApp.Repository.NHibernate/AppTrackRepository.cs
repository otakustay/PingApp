using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate {
    public class AppTrackRepository : IAppTrackRepository {
        private readonly ISession session;

        public AppTrackRepository(ISession session) {
            this.session = session;
        }

        public ICollection<AppTrack> RetrieveByApp(int app) {
            ICollection<AppTrack> tracks = session.QueryOver<AppTrack>()
                .Where(t => t.App.Id == app)
                .List();

            return tracks;
        }

        public int ResetReadStatusByApp(int app) {
            int rows = session.CreateSQLQuery("update AppTrack set HasRead = 0 where App = :app")
                .SetParameter("app", app)
                .ExecuteUpdate();

            return rows;
        }
    }
}
