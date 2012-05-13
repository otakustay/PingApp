using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PingApp.Web.Models.Home {
    public class IndexModel {
        public ICollection<TrackingApp> PriceDecreaseRecommendations { get; set; }

        public ICollection<TrackingApp> PriceFreeRecommendations { get; set; }
    }
}