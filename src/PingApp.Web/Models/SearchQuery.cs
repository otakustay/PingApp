using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;

namespace PingApp.Web.Models {
    public class SearchQuery : PagedQuery<AppBrief> {
        public string Keywords { get; set; }

        public AppSortType Sort { get; set; }

        public string Order { get; set; }

        public DeviceType DeviceType { get; set; }

        public string Category { get; set; }

        public SearchQuery ChangeSort(AppSortType sort) {
            SearchQuery query = Clone();
            query.Sort = sort;
            // 换排序
            if (sort != AppSortType.Relevance) {
                // 同一个排序字段，换排序顺序
                if (Sort == sort) {
                    query.Order = Order == "desc" ? "asc" : "desc";
                }
                // 默认排序顺序
                else {
                    query.Order = (sort == AppSortType.Price || sort == AppSortType.Name) ? "asc" : "desc";
                }
            }
            return query;
        }

        public SearchQuery ChangeDeviceType(DeviceType type) {
            SearchQuery query = Clone();
            query.DeviceType = type;
            return query;
        }

        public SearchQuery ChangeCategory(string category) {
            SearchQuery query = Clone();
            query.Category = category;
            return query;
        }

        public SearchQuery NextPage() {
            SearchQuery query = Clone();
            query.Page = Math.Max(Page + 1, 2);
            return query;
        }

        public SearchQuery PreviousePage() {
            SearchQuery query = Clone();
            query.Page = Math.Max(Page - 1, 1);
            return query;
        }

        private SearchQuery Clone() {
            return new SearchQuery() {
                Keywords = Keywords,
                Sort = Sort,
                Order = Order,
                Category = Category, 
                DeviceType = DeviceType
            };
        }
    }
}