using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Ninject.Activation;
using Ninject.Modules;
using PingApp.Repository.NHibernate.Mapping;
using Ninject;

namespace PingApp.Repository.NHibernate.Dependency {
    public sealed class NHibernateRepositoryModule : NinjectModule {
        private const string SESSION_STORE_KEY = "NHIBERNATE_SESSION";

        public NHibernateRepositoryModule() {
        }

        public override void Load() {
            string connectionString = ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString;
            ISessionFactory sessionFactory = Fluently.Configure()
                .Database(MySQLConfiguration.Standard.ConnectionString(connectionString).ShowSql())
                .Mappings(m => m.FluentMappings.Add<AppUpdateEntityMap>())
                .Mappings(m => m.FluentMappings.Add<AppEntityMap>())
                .Mappings(m => m.FluentMappings.Add<AppBriefEntityMap>())
                .Mappings(m => m.FluentMappings.Add<UserEntityMap>())
                .Mappings(m => m.FluentMappings.Add<AppTrackEntityMap>())
                .Mappings(m => m.FluentMappings.Add<DeveloperComponentMap>())
                .Mappings(m => m.FluentMappings.Add<SellerComponentMap>())
                .Mappings(m => m.FluentMappings.Add<AppUpdateComponentMap>())
                .BuildSessionFactory();

            Bind<ISessionFactory>().ToConstant(sessionFactory);

            Bind<ISession>().ToMethod(OpenSession);
            Bind<IAppRepository>().To<AppRepository>();
            Bind<IAppUpdateRepository>().To<AppUpdateRepository>();
            Bind<IAppTrackRepository>().To<AppTrackRepository>();
        }

        private ISession OpenSession(IContext context) {
            IDictionary store = context.Kernel.Get<IDictionary>();
            if (!store.Contains(SESSION_STORE_KEY)) {
                ISessionFactory factory = context.Kernel.Get<ISessionFactory>();
                ISession session = factory.OpenSession();
                store[SESSION_STORE_KEY] = session;
            }
            return store[SESSION_STORE_KEY] as ISession;
        }
    }
}
