using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Web.Models {
    public class TrackingAppQuery : PagedQuery<TrackingApp> {
        public PagedQuery<AppBrief> OriginalQuery { get; private set; }

        public TrackingAppQuery(PagedQuery<AppBrief> originalQuery, ICollection<AppTrack> tracks) {
            OriginalQuery = originalQuery;

            this.PageIndex = originalQuery.PageIndex;
            this.PageSize = originalQuery.PageSize;

            Dictionary<int, AppTrack> dictionary = tracks.ToDictionary(t => t.App.Id);

            TrackingApp[] trackingApps = originalQuery.Result
                .Select(a => new TrackingApp(a, dictionary.ContainsKey(a.Id) ? dictionary[a.Id] : null))
                .ToArray();
            Fill(trackingApps);
        }
    }
}