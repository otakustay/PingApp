using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;

namespace PingApp.Web.Models {
    public class TitleBuilder {
        public Category Category { get; set; }

        public string Keywords { get; set; }

        public string AppName { get; set; }

        public AppUpdateType UpdateType { get; set; }

        public PriceMode PriceMode { get; set; }

        public float Price { get; set; }

        public DeviceType DeviceType { get; set; }

        public string ForDetail() {
            List<string> items = new List<string>() { 
                AppName, 
                DeviceType.ToString(),
                Category.Name, 
            };
            if (Price == 0) {
                items.Insert(3, "免费");
            }
            return String.Join(" - ", items);
        }

        public string ForList(string type) {
            List<string> items = new List<string>() { type };
            if (!String.IsNullOrEmpty(Keywords)) {
                items.Add(Keywords);
            }
            if (PriceMode == PriceMode.Free) {
                items.Add("免费应用");
            }
            if (UpdateType == AppUpdateType.PriceDecrease) {
                items.Add("特价销售");
            }
            if (UpdateType == AppUpdateType.PriceFree) {
                items.Add("限时免费");
            }
            if (DeviceType != DeviceType.NotProvided) {
                items.Add(DeviceType.ToString());
            }
            if (Category != null) {
                items.Add(Category.Name);
            }
            return String.Join(" - ", items);
        }
    }
}