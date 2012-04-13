using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;
using PingApp.Web.Infrastructures;
using PingApp.Entity;
using PingApp.Web.Models;

namespace PingApp.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801


    public class MvcApplication : System.Web.HttpApplication {

        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.IgnoreRoute("{*robots}", new { robots = @"(.*/)?robots.txt(/.*)?" });

            /*
            routes.MapLowerCaseRoute(
                "ByDeveloper",
                "developer/{id}/{page}",
                new { controller = "Home", action = "ByDeveloper", page = UrlParameter.Optional },
                new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) }
            );

            routes.MapLowerCaseRoute(
                "Signin",
                "signin",
                new { controller = "User", action = "Signin" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) }
            );
            routes.MapLowerCaseRoute(
                "Register",
                "register",
                new { controller = "User", action = "Register" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) }
            );
            routes.MapLowerCaseRoute(
                "Profile",
                "profile",
                new { controller = "User", action = "Profile" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) }
            );
            routes.MapLowerCaseRoute(
                "Authenticate",
                "user/signin",
                new { controller = "User", action = "Authenticate" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "POST" }) }
            );
            routes.MapLowerCaseRoute(
                "Detail",
                "detail/{id}",
                new { controller = "Home", action = "Detail" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) }
            );
            routes.MapLowerCaseRoute(
                "ImportGuide",
                "import/guide",
                new { controller = "User", action = "ImportGuide" }
            );
            routes.MapLowerCaseRoute(
                "ImportConfirm",
                "import/confirm",
                new { controller = "User", action = "ConfirmImport" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) }
            );
            routes.MapLowerCaseRoute(
                "ImportSave",
                "import/confirm",
                new { controller = "User", action = "SaveImport" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "POST" }) }
            );
            routes.MapLowerCaseRoute(
                "ImportInput",
                "import",
                new { controller = "User", action = "Import" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) }
            );
            routes.MapLowerCaseRoute(
                "Import",
                "import",
                new { controller = "User", action = "Import" },
                new { httpMethod = new HttpMethodConstraint(new string[] { "POST" }) }
            );

            MapWishListRoute(routes);
            MapOwnedListRoute(routes);
            MapKeywordsSearchRoute(routes);
            MapIndexSearchRoute(routes);
            */

            routes.MapLowerCaseRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        static MvcApplication() {
        }

        /*
        private static void MapWishListRoute(RouteCollection routes) {
            object route = new { controller = "Track", action = "WishList" };

            // 无参数
            routes.MapLowerCaseRoute("WishList0", "wishlist", route);

            // 1个参数共2种
            routes.MapLowerCaseRoute(
                "WishList1",
                "wishlist/{deviceType}",
                route,
                new { deviceType = new EnumRouteConstraint<DeviceType>() }
            );
            routes.MapLowerCaseRoute(
                "WishList2",
                "wishlist/{category}",
                route,
                new { category = new CategoryRouteConstraint() }
            );

            // 2个参数
            routes.MapLowerCaseRoute(
                "WishList3",
                "wishlist/{deviceType}/{category}",
                route,
                new { deviceType = new EnumRouteConstraint<DeviceType>(), category = new CategoryRouteConstraint() }
            );
        }

        private static void MapOwnedListRoute(RouteCollection routes) {
            object route = new { controller = "Track", action = "OwnedList" };

            // 无参数
            routes.MapLowerCaseRoute("OwnedList0", "owned", route);

            // 1个参数共2种
            routes.MapLowerCaseRoute(
                "OwnedList1",
                "owned/{deviceType}",
                route,
                new { deviceType = new EnumRouteConstraint<DeviceType>() }
            );
            routes.MapLowerCaseRoute(
                "OwnedList2",
                "owned/{category}",
                route,
                new { category = new CategoryRouteConstraint() }
            );

            // 2个参数
            routes.MapLowerCaseRoute(
                "OwnedList3",
                "owned/{deviceType}/{category}",
                route,
                new { deviceType = new EnumRouteConstraint<DeviceType>(), category = new CategoryRouteConstraint() }
            );
        }

        private static void MapKeywordsSearchRoute(RouteCollection routes) {
            object route = new { controller = "Home", action = "Search" };

            // 仅关键词
            routes.MapLowerCaseRoute("KeywordsSearch0", "search/{keywords}", route);

            // 关键词+1个条件共2种
            routes.MapLowerCaseRoute(
                "KeywordsSearch1",
                "search/{deviceType}/{keywords}",
                route,
                new { deviceType = new EnumRouteConstraint<DeviceType>() }
            );
            routes.MapLowerCaseRoute(
                "KeywordsSearch2",
                "search/{category}/{keywords}",
                route,
                new { category = new CategoryRouteConstraint() }
            );

            // 全条件
            routes.MapLowerCaseRoute(
                "KeywordsSearch3",
                "search/{deviceType}/{category}/{keywords}",
                route,
                new {
                    deviceType = new EnumRouteConstraint<DeviceType>(),
                    category = new CategoryRouteConstraint()
                }
            );
        }

        private static void MapIndexSearchRoute(RouteCollection routes) {
            object route = new { controller = "Home", action = "Index" };

            // 无条件
            routes.MapLowerCaseRoute("IndexSearch0", "", route);

            // 1个条件共5种
            routes.MapLowerCaseRoute(
                "IndexSearch1", "{deviceType}", route,
                new {
                    deviceType = new EnumRouteConstraint<DeviceType>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch2", "{category}", route,
                new {
                    category = new CategoryRouteConstraint()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch3", "{priceMode}", route,
                new {
                    priceMode = new EnumRouteConstraint<PriceMode>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch4", "{updateType}", route,
                new {
                    updateType = new EnumRouteConstraint<AppUpdateType>()
                }
            );

            // 2个条件共6种
            routes.MapLowerCaseRoute(
                "IndexSearch5", "{deviceType}/{category}", route,
                new {
                    deviceType = new EnumRouteConstraint<DeviceType>(),
                    category = new CategoryRouteConstraint()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch6", "{deviceType}/{priceMode}", route,
                new {
                    deviceType = new EnumRouteConstraint<DeviceType>(),
                    priceMode = new EnumRouteConstraint<PriceMode>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch7", "{deviceType}/{updateType}", route,
                new {
                    deviceType = new EnumRouteConstraint<DeviceType>(),
                    updateType = new EnumRouteConstraint<AppUpdateType>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch8", "{category}/{priceMode}", route,
                new {
                    category = new CategoryRouteConstraint(),
                    priceMode = new EnumRouteConstraint<PriceMode>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch9", "{category}/{updateType}", route,
                new {
                    category = new CategoryRouteConstraint(),
                    updateType = new EnumRouteConstraint<AppUpdateType>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch10", "{priceMode}/{updateType}", route,
                new {
                    priceMode = new EnumRouteConstraint<PriceMode>(),
                    updateType = new EnumRouteConstraint<AppUpdateType>()
                }
            );

            // 3个条件共4种
            routes.MapLowerCaseRoute(
                "IndexSearch11", "{deviceType}/{category}/{priceMode}", route,
                new {
                    category = new CategoryRouteConstraint(),
                    deviceType = new EnumRouteConstraint<DeviceType>(),
                    priceMode = new EnumRouteConstraint<PriceMode>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch12", "{deviceType}/{category}/{updateType}", route,
                new {
                    category = new CategoryRouteConstraint(),
                    deviceType = new EnumRouteConstraint<DeviceType>(),
                    updateType = new EnumRouteConstraint<AppUpdateType>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch13", "{deviceType}/{priceMode}/{updateType}", route,
                new {
                    deviceType = new EnumRouteConstraint<DeviceType>(),
                    priceMode = new EnumRouteConstraint<PriceMode>(),
                    updateType = new EnumRouteConstraint<AppUpdateType>()
                }
            );
            routes.MapLowerCaseRoute(
                "IndexSearch14", "{category}/{priceMode}/{updateType}", route,
                new {
                    category = new CategoryRouteConstraint(),
                    priceMode = new EnumRouteConstraint<PriceMode>(),
                    updateType = new EnumRouteConstraint<AppUpdateType>()
                }
            );

            // 所有条件
            routes.MapLowerCaseRoute("IndexSearch15", "{deviceType}/{category}/{priceMode}/{updateType}", route);
        }
        */
    }
}