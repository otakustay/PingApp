using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace PingApp.Repository.NHibernate.Dependency {
    public class SessionStore : Dictionary<string, ISession>, IDisposable {
        public ISession Session {
            get {
                return Values.FirstOrDefault();
            }
        }

        public SessionStore() {
        }

        public void Dispose() {
            DiscardCurrentSession();
        }

        public ITransaction BeginTransaction() {
            return Session == null ? null : Session.BeginTransaction();
        }

        public void CommitTransaction() {
            if (Session != null && Session.Transaction != null && Session.Transaction.IsActive) {
                Session.Transaction.Commit();
            }
        }

        public void RollbackTransaction() {
            if (Session != null && Session.Transaction != null && Session.Transaction.IsActive) {
                Session.Transaction.Rollback();
            }
        }

        public void DiscardCurrentSession() {
            if (Session != null) {
                Session.Flush();
                Session.Dispose();
                Clear();
            }
        }
    }
}
