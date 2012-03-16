using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;

namespace PingApp.Web.Models {
    public class TrackQuery : PagedQuery<AppTrack> {
        private AppSortType sort = AppSortType.Name;

        public DeviceType DeviceType { get; set; }

        public string Category { get; set; }

        public AppSortType Sort {
            get {
                return sort;
            }
            set {
                if (value == AppSortType.Relevance) {
                    sort = AppSortType.Name;
                }
                else {
                    sort = value;
                }
            }
        }

        public string Order { get; set; }

        public TrackQuery ChangeDeviceType(DeviceType type) {
            TrackQuery query = Clone();
            query.DeviceType = type;
            return query;
        }

        public TrackQuery ChangeCategory(string category) {
            TrackQuery query = Clone();
            query.Category = category;
            return query;
        }

        public TrackQuery ChangeSort(AppSortType type) {
            TrackQuery query = Clone();
            query.Sort = type;
            // 同一个排序字段，换排序顺序
            if (Sort == type) {
                query.Order = Order == "desc" ? "asc" : "desc";
            }
            // 默认排序顺序
            else {
                query.Order = (type == AppSortType.Price || type == AppSortType.Name) ? "asc" : "desc";
            }
            return query;
        }

        public TrackQuery NextPage() {
            TrackQuery query = Clone();
            query.Page = Math.Max(Page + 1, 2);
            return query;
        }

        public TrackQuery PreviousePage() {
            TrackQuery query = Clone();
            query.Page = Math.Max(Page - 1, 1);
            return query;
        }

        public TrackQuery Clone() {
            return new TrackQuery() {
                DeviceType = DeviceType,
                Category = Category
            };
        }
    }
}