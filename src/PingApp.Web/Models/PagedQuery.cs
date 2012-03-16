using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Web.Infrastructures;
using Newtonsoft.Json;

namespace PingApp.Web.Models {
    public class PagedQuery<T> {
        private int pageSize = Default.PageSize;

        private int page = 1;

        public int PageSize {
            get {
                return pageSize;
            }
            set {
                pageSize = value;
            }
        }

        public int Page {
            get {
                return page;
            }
            set {
                page = Math.Max(value, 1);
            }
        }

        public IEnumerable<T> Result { get; private set; }

        public bool HasNextPage { get; private set; }

        public bool HasPreviousPage {
            get {
                return Page > 1;
            }
        }

        [JsonIgnore]
        public int StartIndex {
            get {
                return (Math.Max(Page, 1) - 1) * PageSize;
            }
        }

        [JsonIgnore]
        public int EndIndex {
            get {
                return StartIndex + PageSize;
            }
        }

        [JsonIgnore]
        public int TakeSize {
            get {
                return PageSize + 1;
            }
        }

        public void Fill(ICollection<T> result) {
            HasNextPage = result.Count > PageSize;
            Result = result.Take(PageSize);
        }
    }
}