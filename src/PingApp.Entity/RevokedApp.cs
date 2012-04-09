using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class RevokedApp : App {
        public DateTime RevokeTime { get; set; }

        public RevokedApp(App source) {
            Id = source.Id;
            Brief = new AppBrief() { Id = source.Id };
            UpdateFrom(source);
        }
    }
}
