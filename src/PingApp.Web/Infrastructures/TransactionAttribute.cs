using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;
using NLog;

namespace PingApp.Web.Infrastructures {
    public class TransactionAttribute : ActionFilterAttribute {
        private ITransaction transaction;

        private Logger logger = LogManager.GetCurrentClassLogger();

        public override void OnActionExecuted(ActionExecutedContext filterContext) {
            if (transaction != null) {
                if (transaction.IsActive) {
                    transaction.Commit();
                }
                else {
                    logger.Warn("Transaction disposed");
                }
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            BaseController controller = filterContext.Controller as BaseController;
            if (controller != null) {
                transaction = controller.DbSession.BeginTransaction();
            }
        }
    }
}