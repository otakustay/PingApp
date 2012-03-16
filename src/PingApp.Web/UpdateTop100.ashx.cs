using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.IO;
using NLog;

namespace PingApp.Web {
    /// <summary>
    /// Summary description for UpdateTop100
    /// </summary>
    public class UpdateTop100 : IHttpHandler {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public void ProcessRequest(HttpContext context) {
            context.Response.ContentType = "application/json";
            logger.Trace("{0}: {1}", context.Request.HttpMethod, context.Request.Url);

            if (context.Request.HttpMethod.ToUpper() == "GET") {
                ICacheManager cache = EnterpriseLibraryContainer.Current.GetInstance<ICacheManager>();
                IEnumerable<int> apps = cache.GetData("Top100Apps") as IEnumerable<int>;
                context.Response.Write("[" + String.Join(", ", apps ?? new int[0]) + "]");
                return;
            }

            logger.Info("Received top100 update");
            string content;
            using (StreamReader reader = new StreamReader(context.Request.InputStream)) {
                content = reader.ReadToEnd();
            }

            if (String.IsNullOrEmpty(content)) {
                context.Response.Write("No content provided");
                return;
            }

            try {
                IEnumerable<int> apps = content.Split(',').Select(s => Convert.ToInt32(s));
                ICacheManager cache = EnterpriseLibraryContainer.Current.GetInstance<ICacheManager>();
                cache.Add(
                    "Top100Apps",
                    new HashSet<int>(apps),
                    CacheItemPriority.NotRemovable,
                    null
                );
                logger.Info("{0} apps cached to top100", apps.Count());
                context.Response.Write("true");
            }
            catch (Exception ex) {
                context.Response.Write(ex);
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}