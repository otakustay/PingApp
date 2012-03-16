using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Web.Models;
using System.Web.Mvc;
using PingApp.Entity;

namespace PingApp.Web.Infrastructures {
    public static class UrlExtension {
        public static string Home(this UrlHelper helper, ListQuery query) {
            string path = "~";
            if (query.DeviceType != DeviceType.NotProvided) {
                path += "/" + query.DeviceType.ToString().ToLower();
            }
            if (!String.IsNullOrEmpty(query.Category)) {
                path += "/" + query.Category.ToString();
            }
            if (query.PriceMode != PriceMode.All) {
                path += "/" + query.PriceMode.ToString().ToLower();
            }
            if (query.UpdateType.HasValue) {
                path += "/" + query.UpdateType.ToString().ToLower();
            }
            if (query.Page > 1) {
                path += "?page=" + query.Page;
            }

            return helper.Content(path);
        }

        public static string Search(this UrlHelper helper, SearchQuery query) {
            string path = "~/search";
            if (query.DeviceType != DeviceType.NotProvided) {
                path += "/" + query.DeviceType.ToString().ToLower();
            }
            if (!String.IsNullOrEmpty(query.Category)) {
                path += "/" + query.Category.ToString();
            }
            if (!String.IsNullOrEmpty(query.Keywords)) {
                path += "/" + query.Keywords;
            }

            bool queryStringInitialized = false;
            if (query.Page > 1) {
                path += "?page=" + query.Page;
                queryStringInitialized = true;
            }
            if (query.Sort != AppSortType.Relevance) {
                path += (queryStringInitialized ? "&" : "?") + "sort=" + query.Sort.ToString().ToLower();
                queryStringInitialized = true;
            }
            if (query.Sort != AppSortType.Relevance) {
                path += (queryStringInitialized ? "&" : "?") + "order=" + (query.Order == "desc" ? "desc" : "asc");
                queryStringInitialized = true;
            }

            return helper.Content(path);
        }

        public static string TrackList(this UrlHelper helper, AppTrackStatus status, TrackQuery query) {
            string path = status == AppTrackStatus.InWish ? "~/wishlist" : "~/owned";
            if (query.DeviceType != DeviceType.NotProvided) {
                path += "/" + query.DeviceType.ToString().ToLower();
            }
            if (!String.IsNullOrEmpty(query.Category)) {
                path += "/" + query.Category.ToString();
            }

            path += "?sort=" + query.Sort.ToString().ToLower();
            path += "&order=" + (query.Order == "desc" ? "desc" : "asc");
            path += "&page=" + query.Page;

            return helper.Content(path);
        }

        public static string ByDeveloper(this UrlHelper helper, int developerId, int page) {
            return helper.Content(String.Format("~/developer/{0}/{1}", developerId, page));
        }

        public static string Share(this UrlHelper helper, string type, App app) {
            string deviceType = app.Brief.DeviceType == DeviceType.Universal ? "通用" : app.Brief.DeviceType.ToString().ToLower();
            string[] updateTypes = { "新近上架", "应用发掘", "应用发掘", "特价销售", "限时免费", "应用发掘", "应用发掘" };
            string updateType = updateTypes[(int)app.Brief.LastValidUpdate.Type];
            string image = (app.ScreenshotUrls ?? app.IPadScreenshotUrls).FirstOrDefault();

            string content = String.Format(
                "{0}-{1}：{2}。查看应用：http://www.pingapp.net/detail/{3}。更多内容：http://www.pingapp.net",
                updateType, deviceType, app.Brief.Name, app.Id
            );
            return String.Format(
                "http://www.jiathis.com/send/?webid={0}&url=&title={1}&uid=1532409&pic={2}",
                type, helper.Encode(content), helper.Encode(image)
            );
        }
    }
}