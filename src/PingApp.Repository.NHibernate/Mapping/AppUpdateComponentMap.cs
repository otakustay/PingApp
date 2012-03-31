using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate.Mapping {
    public class AppUpdateComponentMap : ComponentMap<AppUpdate> {
        public AppUpdateComponentMap() {
            Map(u => u.App).Column("Id").ReadOnly();
            Map(u => u.NewValue).Column("LastValidUpdateNewValue");
            Map(u => u.OldValue).Column("LastValidUpdateOldValue");
            Map(u => u.Time).Column("LastValidUpdateTime");
            Map(u => u.Type).Column("LastValidUpdateType").CustomType<AppUpdateType>();
        }
    }
}