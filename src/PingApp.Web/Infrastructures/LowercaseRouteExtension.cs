using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;

namespace PingApp.Web.Infrastructures {
    public static class LowercaseRouteExtension {
        public static LowerCaseRoute MapLowerCaseRoute(this RouteCollection routes, string name, string url) {
            return routes.MapLowerCaseRoute(name, url, null, null);
        }
        public static LowerCaseRoute MapLowerCaseRoute(this RouteCollection routes, string name, string url, object defaults) {
            return routes.MapLowerCaseRoute(name, url, defaults, null);
        }
        public static LowerCaseRoute MapLowerCaseRoute(this RouteCollection routes, string name, string url, string[] namespaces) {
            return routes.MapLowerCaseRoute(name, url, null, null, namespaces);
        }
        public static LowerCaseRoute MapLowerCaseRoute(this RouteCollection routes, string name, string url, object defaults, object constraints) {
            return routes.MapLowerCaseRoute(name, url, defaults, constraints, null);
        }
        public static LowerCaseRoute MapLowerCaseRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces) {
            return routes.MapLowerCaseRoute(name, url, defaults, null, namespaces);
        }
        public static LowerCaseRoute MapLowerCaseRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces) {
            if (routes == null) throw new ArgumentNullException("routes");
            if (url == null) throw new ArgumentNullException("url");
            LowerCaseRoute route2 = new LowerCaseRoute(url, new MvcRouteHandler());
            route2.Defaults = new RouteValueDictionary(defaults);
            route2.Constraints = new RouteValueDictionary(constraints);
            route2.DataTokens = new RouteValueDictionary();
            LowerCaseRoute item = route2;
            if ((namespaces != null) && (namespaces.Length > 0))
                item.DataTokens["Namespaces"] = namespaces;
            routes.Add(name, item);
            return item;
        }

    }
}