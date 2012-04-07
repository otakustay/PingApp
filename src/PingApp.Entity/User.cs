using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class User {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public string Description { get; set; }

        public string Website { get; set; }

        public string Password { get; set; }

        public DateTime RegisterTime { get; set; }

        public int PreferredLanguagePriority { get; set; }

        public UserStatus Status { get; set; }

        #region 通知设置

        public bool NotifyOnWishPriceDrop { get; set; }

        public bool NotifyOnWishFree { get; set; }

        public bool NotifyOnWishUpdate { get; set; }

        public bool NotifyOnOwnedUpdate { get; set; }

        public bool ReceiveSiteUpdates { get; set; }

        #endregion

        public User() {
            Description = String.Empty;
            Website = String.Empty;
        }
    }
}
