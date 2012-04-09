using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;
using System.Web.Security;
using PingApp.Entity;
using NHibernate.Criterion;
using NHibernate.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;

namespace PingApp.Web.Infrastructures {
    public class BaseController : Controller {
        private ISession session;

        private readonly object syncRoot = new object();

        public ISession DbSession {
            get {
                if (session == null) {
                    lock (syncRoot) {
                        if (session == null) {
                            session = MvcApplication.OpenSession();
                            HttpContext.Items["NHibernateSession"] = session;
                        }
                    }
                }
                return session;
            }
        }

        protected User CurrentUser {
            get {
                // 用户信息
                if (User.Identity.IsAuthenticated) {
                    User user = Session["CurrentUser"] as User;
                    if (user == null) {
                        user = DbSession.Get<User>(Convert.ToInt32(User.Identity.Name));
                        Session["CurrentUser"] = user;
                    }
                    return user;
                }
                else {
                    return null;
                }
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            base.OnActionExecuting(filterContext);

            if (User.Identity.IsAuthenticated) {
                ViewBag.Username = CurrentUser.Username;
            }

            ICacheManager cache = EnterpriseLibraryContainer.Current.GetInstance<ICacheManager>();
            IDictionary<AppUpdateType, int> updates;
            if (cache.Contains("AppUpdateStatistics")) {
                updates = cache.GetData("AppUpdateStatistics") as IDictionary<AppUpdateType, int>;
            }
            else {
                updates =
                    DbSession.Query<AppUpdate>()
                    .Where(u => u.Time >= DateTime.Today.AddDays(-1) && u.Time < DateTime.Today)
                    .GroupBy(u => u.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionary(g => g.Type, g => g.Count);
                cache.Add(
                    "AppUpdateStatistics",
                    updates,
                    CacheItemPriority.NotRemovable,
                    null,
                    new AbsoluteTime(DateTime.Today.AddDays(1).AddMinutes(1))
                );
            }
            // 应用总数怎么办
            int appCount;
            if (cache.Contains("AppCount")) {
                appCount = (int)cache.GetData("AppCount");
            }
            else {
                object[] values = (object[])DbSession.CreateSQLQuery("show table status like 'AppBrief'").UniqueResult();
                appCount = (int)((ulong)values[4]);
                cache.Add(
                    "AppCount",
                    appCount,
                    CacheItemPriority.NotRemovable,
                    null,
                    new AbsoluteTime(DateTime.Now.AddHours(3))
                );
            }

            ViewBag.NewAppCount = updates.ContainsKey(AppUpdateType.New) ? updates[AppUpdateType.New] : 0;
            ViewBag.UpdatedAppCount = updates.ContainsKey(AppUpdateType.NewRelease) ? updates[AppUpdateType.NewRelease] : 0;
            ViewBag.OffAppCount = updates.ContainsKey(AppUpdateType.Revoke) ? updates[AppUpdateType.Revoke] : 0;
            ViewBag.AppCount = appCount;
            ViewBag.Top100 = cache.GetData("Top100Apps") ?? new HashSet<int>();
            ViewBag.IsDebug = HttpContext.IsDebuggingEnabled;
            ViewBag.User = CurrentUser;
        }
    }
}