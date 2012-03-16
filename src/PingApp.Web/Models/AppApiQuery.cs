using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;

namespace PingApp.Web.Models {
    public class AppApiQuery : PagedQuery<AppBrief> {
        public string Category { get; set; }

        public bool FreeOnly { get; set; }

        public DeviceType DeviceType { get; set; }

        public int Developer { get; set; }
    }
}