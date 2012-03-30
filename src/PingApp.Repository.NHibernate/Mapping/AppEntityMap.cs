using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate.Mapping {
    class AppEntityMap : ClassMap<App> {
        public AppEntityMap() {
            Id(a => a.Id);
            Map(a => a.AverageUserRating);
            Map(a => a.Categories).CustomType<CategoryArrayType>();
            Map(a => a.CensoredName);
            Map(a => a.ContentAdvisoryRating);
            Map(a => a.ContentRating);
            Map(a => a.Description);
            Map(a => a.IPadScreenshotUrls).CustomType<StringArrayType>();
            Map(a => a.Languages).CustomType<StringArrayType>();
            Map(a => a.LargeIconUrl);
            Map(a => a.ReleaseNotes);
            Map(a => a.ScreenshotUrls).CustomType<StringArrayType>();
            Component(a => a.Seller).ColumnPrefix("Seller");
            Map(a => a.UserRatingCount);

            HasOne(a => a.Brief).Constrained();

            Not.LazyLoad();
        }
    }
}
