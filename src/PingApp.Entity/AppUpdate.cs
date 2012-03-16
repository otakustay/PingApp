using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class AppUpdate {
        public int Id { get; set; }

        public int App { get; set; }

        public DateTime Time { get; set; }

        public AppUpdateType Type { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public bool IsPriceUpdate {
            get {
                return Type == AppUpdateType.PriceDecrease ||
                    Type == AppUpdateType.PriceFree ||
                    Type == AppUpdateType.PriceIncrease;
            }
        }

        public AppUpdate() {
            OldValue = String.Empty;
            NewValue = String.Empty;
        }

        public static bool IsValidUpdate(AppUpdateType type) {
            return type != AppUpdateType.AddToNote &&
                type != AppUpdateType.Off;
        }
    }
}
