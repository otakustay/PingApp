using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate.Mapping {
    public class UserEntityMap : ClassMap<User> {
        public UserEntityMap() {
            Id(u => u.Id).GeneratedBy.GuidComb();
            Map(u => u.Description);
            Map(u => u.Email);
            Map(u => u.NotifyOnOwnedUpdate);
            Map(u => u.NotifyOnWishFree);
            Map(u => u.NotifyOnWishPriceDrop);
            Map(u => u.NotifyOnWishUpdate);
            Map(u => u.ReceiveSiteUpdates);
            Map(u => u.Username);
            Map(u => u.Website);
            Map(u => u.Password);
            Map(u => u.RegisterTime);
            Map(u => u.PreferredLanguagePriority);
            Map(u => u.Status).CustomType<UserStatus>();

            Not.LazyLoad();
        }
    }
}