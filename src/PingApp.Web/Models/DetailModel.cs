using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;

namespace PingApp.Web.Models {
    public class DetailModel {
        public App App { get; set; }

        public bool Owned { get; set; }

        public bool InWish { get; set; }

        public IEnumerable<AppUpdate> Updates { get; set; }

        public IEnumerable<AppBrief> RelativeApps { get; set; }
    }
}