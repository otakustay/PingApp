using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;

namespace PingApp.Web.Models {
    public class ImportModel  {
        public ICollection<AppBrief> Apps { get; set; }

        public ICollection<string> NotFound { get; set; }
    }
}