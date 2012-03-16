using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace PingApp.Web.Infrastructures {
    public class EnumRouteConstraint<TEnum> : IRouteConstraint where TEnum : struct {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (values[parameterName] == null) {
                return false;
            }
            string value = values[parameterName].ToString();
            TEnum t;
            bool result = Enum.TryParse<TEnum>(value, true, out t);
            return result && Enum.IsDefined(typeof(TEnum), t);
        }
    }
}