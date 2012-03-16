using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PingApp.Web.Infrastructures;

namespace PingApp.Web.Controllers {
    [Region("About")]
    public class AboutController : BaseController {
        public ActionResult Site() {
            ViewBag.Title = "关于本站";
            return View("Site");
        }

        public ActionResult Usage() {
            ViewBag.Title = "利用PingApp更省钱地享受iOS应用";
            return View("Usage");
        }
    }
}
