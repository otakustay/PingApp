using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Repository.Quries {
    public class PagedQuery<T> : ListQuery<T> {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int SkipSize {
            get {
                return (PageIndex - 1) * PageSize;
            }
        }

        public int TakeSize {
            get {
                return PageSize + 1;
            }
        }

        public bool HasNextPage { get; private set; }

        public override void Fill(ICollection<T> result) {
            base.Fill(result.Take(PageSize).ToArray());
            HasNextPage = result.Count > PageSize;
        }
    }
}
