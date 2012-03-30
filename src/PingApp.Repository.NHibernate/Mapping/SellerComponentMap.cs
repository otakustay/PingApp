using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using FluentNHibernate.Mapping;

namespace PingApp.Repository.NHibernate.Mapping {
    class SellerComponentMap : ComponentMap<Seller> {
        public SellerComponentMap() {
            Map(s => s.Name);
            Map(s => s.ViewUrl);
        }
    }
}
