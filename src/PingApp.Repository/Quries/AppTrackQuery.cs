using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository.Quries {
    public class AppTrackQuery : PagedQuery<AppTrack> {
        public Guid User { get; set; }

        public AppTrackStatus Status { get; set; }

        public AppTrackQuery(int pageIndex, int pageSize)
            : base(pageIndex, pageSize) {
        }
    }
}
