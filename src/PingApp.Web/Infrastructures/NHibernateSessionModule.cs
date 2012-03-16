using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate;

namespace PingApp.Web.Infrastructures {
    public class NHibernateSessionModule : IHttpModule {
        public void Dispose() {
        }

        public void Init(HttpApplication context) {
            context.EndRequest += new EventHandler(DisposeSession);
            context.Error += new EventHandler(DisposeSession);
        }

        void DisposeSession(object sender, EventArgs e) {
            ISession session = HttpContext.Current.Items["NHibernateSession"] as ISession;
            if (session != null) {
                try {
                    session.Flush();
                    session.Dispose();
                }
                catch (Exception) {
                }
            }
        }
    }
}