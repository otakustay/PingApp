using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Ninject;
using PingApp.Entity;
using PingApp.Repository;

namespace PingApp.Web.Infrastructures {
    public class BaseController : Controller {
        private const string CURRENT_USER_KEY = "CurrentUser";

        [Inject]
        public RepositoryEmitter Repository { get; set; }

        protected User CurrentUser {
            get {
                // 用户信息
                if (User.Identity.IsAuthenticated) {
                    User user = Session[CURRENT_USER_KEY] as User;
                    if (user == null) {
                        user = Repository.User.Retrieve(Guid.Parse(User.Identity.Name));
                        Session[CURRENT_USER_KEY] = user;
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

            ViewBag.IsDebug = HttpContext.IsDebuggingEnabled;
            ViewBag.User = CurrentUser;
            ViewBag.Top100 = new HashSet<int>();
        }
    }
}