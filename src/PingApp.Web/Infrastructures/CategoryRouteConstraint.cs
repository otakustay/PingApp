using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using PingApp.Entity;

namespace PingApp.Web.Infrastructures {
    public class CategoryRouteConstraint : IRouteConstraint {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (values[parameterName] == null) {
                return false;
            }
            string value = values[parameterName].ToString();
            return Category.Get(value) != null;
        }
    }
}