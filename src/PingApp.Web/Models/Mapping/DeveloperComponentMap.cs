using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using FluentNHibernate.Mapping;

namespace PingApp.Web.Models.Mapping {
    class DeveloperComponentMap : ComponentMap<Developer> {
        public DeveloperComponentMap() {
            Map(d => d.Id);
            Map(d => d.Name);
            Map(d => d.ViewUrl);
        }
    }
}
