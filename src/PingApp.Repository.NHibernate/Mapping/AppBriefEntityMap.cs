using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using FluentNHibernate.Mapping;
using NHibernate.UserTypes;
using NHibernate;
using NHibernate.SqlTypes;
using System.Data;

namespace PingApp.Repository.NHibernate.Mapping {
    class AppBriefEntityMap : ClassMap<AppBrief> {
        public AppBriefEntityMap() {
            Id(b => b.Id);
            Map(b => b.AverageUserRatingForCurrentVersion);
            Map(b => b.Introduction);
            Map(b => b.ReleaseNotes);
            Map(b => b.Currency);
            Map(b => b.DeviceType).CustomType<DeviceType>().Access.ReadOnly();
            Map(b => b.Features).CustomType<StringArrayType>();
            Map(b => b.FileSize);
            Map(b => b.IconUrl);
            Map(b => b.IsGameCenterEnabled).Access.ReadOnly();
            Map(b => b.Name);
            Map(b => b.Price);
            Map(b => b.PrimaryCategory).CustomType<CategoryType>();
            Map(b => b.ReleaseDate);
            Map(b => b.SupportedDevices).CustomType<StringArrayType>();
            Map(b => b.UserRatingCountForCurrentVersion);
            Map(b => b.Version);
            Map(b => b.ViewUrl);
            Map(b => b.LanguagePriority);
            Map(b => b.IsActive, "IsValid");
            Component(b => b.Developer).ColumnPrefix("Developer");
            Component(b => b.LastValidUpdate);

            Not.LazyLoad();
        }
    }
}
