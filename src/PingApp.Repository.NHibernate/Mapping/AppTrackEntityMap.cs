using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;
using FluentNHibernate.Mapping;

namespace PingApp.Repository.NHibernate.Mapping {
    public class AppTrackEntityMap : ClassMap<AppTrack> {
        public AppTrackEntityMap() {
            Id(t => t.Id).GeneratedBy.GuidComb();
            Map(t => t.User);
            Map(t => t.Status).CustomType<AppTrackStatus>();
            Map(t => t.Rate);
            Map(t => t.HasRead);
            Map(t => t.CreateTime);
            Map(t => t.CreatePrice);
            Map(t => t.BuyTime);
            Map(t => t.BuyPrice);

            References(t => t.App, "App").Fetch.Select();

            Not.LazyLoad();
        }
    }
}