using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PingApp.Web.Infrastructures;
using PingApp.Entity;
using NHibernate.Linq;

namespace PingApp.Web.Controllers {
    public class ValidateController : BaseController {
        public ActionResult Email(string email) {
            bool exists = DbSession.Query<User>().Any(u => u.Email == email);

            // 如果已经存在了，缓存住，下次别再来查了
            if (exists) {
                Response.Cache.SetExpires(DateTime.Now.AddDays(1)); 
            }

            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = !exists;
            return result;
        }

        public ActionResult Username(string username) {
            bool exists = DbSession.Query<User>().Any(u => u.Username == username);

            // 如果已经存在了，缓存住，下次别再来查了
            if (exists) {
                Response.Cache.SetExpires(DateTime.Now.AddDays(1));
            }

            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = !exists;
            return result;
        }
    }
}