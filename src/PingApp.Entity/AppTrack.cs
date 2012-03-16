using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class AppTrack {
        public int Id { get; set; }

        public int User { get; set; }

        public AppBrief App { get; set; }

        public AppTrackStatus Status { get; set; }

        public DateTime CreateTime { get; set; }

        public float CreatePrice { get; set; }

        public DateTime? BuyTime { get; set; }

        public float? BuyPrice { get; set; }

        public int Rate { get; set; }

        public bool HasRead { get; set; }

        public bool RequireNotification(User user, AppUpdateType type) {
            if (Status == AppTrackStatus.Owned) {
                return user.NotifyOnOwnedUpdate && type == AppUpdateType.NewRelease;
            }
            else {
                return (user.NotifyOnWishFree && type == AppUpdateType.PriceFree) ||
                    (user.NotifyOnWishPriceDrop && type == AppUpdateType.PriceDecrease) ||
                    (user.NotifyOnWishUpdate && type == AppUpdateType.NewRelease);
            }
        }
    }
}
