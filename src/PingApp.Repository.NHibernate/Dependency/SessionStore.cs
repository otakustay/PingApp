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
            Console.WriteLine("SessionStore: Session store constructed");
        }

        public void Dispose() {
            Console.WriteLine("SessionStore: Session store disposed");
            DiscardCurrentSession();
        }

        public ITransaction BeginTransaction() {
            return Session == null ? null : Session.BeginTransaction();
        }

        public void CommitTransaction() {
            if (Session != null && Session.Transaction != null && Session.Transaction.IsActive) {
                Console.WriteLine("SessionStore: Commit transaction");
                Session.Transaction.Commit();
            }
        }

        public void RollbackTransaction() {
            if (Session != null && Session.Transaction != null && Session.Transaction.IsActive) {
                Console.WriteLine("SessionStore: Rollback transaction");
                Session.Transaction.Rollback();
            }
        }

        public void DiscardCurrentSession() {
            Console.WriteLine("SessionStore: Discard current session");
            if (Session != null) {
                CommitTransaction();
                Session.Flush();
                Session.Dispose();
            }
        }
    }
}
