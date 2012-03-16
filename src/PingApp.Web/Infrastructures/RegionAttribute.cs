using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PingApp.Web.Infrastructures {
    public class RegionAttribute : ActionFilterAttribute {
        public string Region { get; set; }

        public RegionAttribute(string region) {
            Region = region;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            filterContext.Controller.ViewBag.Region = Region;
        }
    }
}