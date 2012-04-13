using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository.Quries {
    public class AppUpdateQuery : ListQuery<AppUpdate> {
        public int App { get; set; }

        public DateTime LatestTime { get; set; }

        public DateTime? EarliestTime { get; set; }
    }
}
