using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository.Quries {
    public class DeveloperAppsQuery : PagedQuery<AppBrief> {
        public Developer Developer { get; set; }
    }
}
