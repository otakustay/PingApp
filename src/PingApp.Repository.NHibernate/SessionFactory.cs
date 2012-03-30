using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using PingApp.Repository.NHibernate.Mapping;

namespace PingApp.Repository.NHibernate {
    public static class SessionFactory {
        private static readonly ISessionFactory factory;

        static SessionFactory() {
            string connectionString = ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString;
            factory = Fluently.Configure()
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
        }

        public static ISession OpenSession() {
            return factory.OpenSession();
        }
    }
}
