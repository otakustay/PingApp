using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PingApp.Entity;
using PingApp.Web.Infrastructures;
using NHibernate;
using PingApp.Web.Models;

namespace PingApp.Web.Controllers {
    public class ApiController : BaseController {
        public ActionResult List(AppApiQuery query) {
            IQueryOver<AppBrief, AppBrief> search = DbSession.QueryOver<AppBrief>();
            if (!String.IsNullOrEmpty(query.Category)) {
                search = search.Where(a => a.PrimaryCategory == Category.Get(query.Category));
            }
            if (query.Developer != 0) {
                search = search.Where(a => a.Developer.Id == query.Developer);
            }
            if (query.DeviceType != DeviceType.NotProvided) {
                search = search.Where(a => a.DeviceType == query.DeviceType || a.DeviceType == DeviceType.Universal);
            }
            if (query.FreeOnly) {
                search = search.Where(a => a.Price == 0);
            }

            IList<AppBrief> list = search
                .OrderBy(a => a.LastValidUpdate.Time).Desc
                .Skip(query.StartIndex)
                .Take(query.TakeSize)
                .List();

            query.Fill(list);

            return new NewtonJsonActionResult(query);
        }

        public ActionResult Get(int id) {
            App app = DbSession.Get<App>(id);

            return new NewtonJsonActionResult(app);
        }
    }
}