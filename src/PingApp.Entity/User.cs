using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class User : IUpdateTarget {
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

        public void UpdateFrom(object obj) {
            User newOne = obj as User;
            if (newOne == null || newOne == this || newOne.Id != Id) {
                return;
            }

            Description = newOne.Description ?? String.Empty;
            Website = newOne.Website ?? String.Empty;
            NotifyOnOwnedUpdate = newOne.NotifyOnOwnedUpdate;
            NotifyOnWishFree = newOne.NotifyOnWishFree;
            NotifyOnWishPriceDrop = newOne.NotifyOnWishPriceDrop;
            NotifyOnWishUpdate = newOne.NotifyOnWishUpdate;
            PreferredLanguagePriority = newOne.PreferredLanguagePriority;
        }
    }
}
