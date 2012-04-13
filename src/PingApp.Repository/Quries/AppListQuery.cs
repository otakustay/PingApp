using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository.Quries {
    public class AppListQuery : PagedQuery<AppBrief> {
        public DeviceType DeviceType { get; set; }

        public string Category { get; set; }

        public PriceMode PriceMode { get; set; }

        public AppUpdateType? UpdateType { get; set; }

        public AppListQuery(int pageIndex, int pageSize)
            : base(pageIndex, pageSize) {
        }
    }
}
