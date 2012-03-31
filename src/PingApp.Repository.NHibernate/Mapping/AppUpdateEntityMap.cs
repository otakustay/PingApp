using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate.Mapping {
    public class AppUpdateEntityMap : ClassMap<AppUpdate> {
        public AppUpdateEntityMap() {
            Id(u => u.Id).GeneratedBy.Increment();
            Map(u => u.App);
            Map(u => u.NewValue);
            Map(u => u.OldValue);
            Map(u => u.Time);
            Map(u => u.Type).CustomType<AppUpdateType>();

            Not.LazyLoad();
        }
    }
}