using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate {
    public class AppUpdateRepository : IAppUpdateRepository {
        private readonly ISession session;

        public AppUpdateRepository(ISession session) {
            this.session = session;
        }

        public void Save(AppUpdate update) {
            session.Save(update);
        }
    }
}
