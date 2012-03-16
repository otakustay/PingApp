using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;
using System.Web.Routing;

namespace PingApp.Web.Models {
    public class ListQuery : PagedQuery<AppBrief> {
        public DeviceType DeviceType { get; set; }

        public string Category { get; set; }

        public PriceMode PriceMode { get; set; }

        public AppUpdateType? UpdateType { get; set; }

        public ListQuery ChangeDeviceType(DeviceType type) {
            ListQuery query = Clone();
            query.DeviceType = type;
            return query;
        }

        public ListQuery ChangeCategory(string category) {
            ListQuery query = Clone();
            query.Category = category;
            return query;
        }

        public ListQuery ChangePriceMode(PriceMode mode) {
            ListQuery query = Clone();
            query.PriceMode = mode;
            return query;
        }

        public ListQuery ChangeUpdateType(AppUpdateType? type) {
            ListQuery query = Clone();
            query.UpdateType = type;
            return query;
        }

        public ListQuery NextPage() {
            ListQuery query = Clone();
            query.Page = Math.Max(Page + 1, 2);
            return query;
        }

        public ListQuery PreviousePage() {
            ListQuery query = Clone();
            query.Page = Math.Max(Page - 1, 1);
            return query;
        }

        private ListQuery Clone() {
            return new ListQuery() {
                DeviceType = DeviceType,
                Category = Category,
                PriceMode = PriceMode,
                UpdateType = UpdateType
            };
        }
    }
}