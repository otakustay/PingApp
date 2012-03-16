using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PingApp.Web.Infrastructures;
using PingApp.Entity;
using PingApp.Web.Models;
using NHibernate;
using NHibernate.Criterion.Lambda;
using System.Text;

namespace PingApp.Web.Controllers {
    [Authorize]
    public class TrackController : BaseController {
        [HttpGet]
        [Region("WishList")]
        public ActionResult WishList(TrackQuery query, int page = 1) {
            return List(query, AppTrackStatus.InWish, page);
        }

        [HttpGet]
        [Region("Owned")]
        public ActionResult OwnedList(TrackQuery query, int page = 1) {
            return List(query, AppTrackStatus.Owned, page);
        }

        [HttpPost]
        [Transaction]
        public ActionResult Save(AppTrack track) {
            JsonResult result = new JsonResult();
            result.ContentEncoding = Encoding.UTF8;
            result.ContentType = "application/json";

            // 判断对应的应用有没有
            AppBrief app = DbSession.Get<AppBrief>(track.App.Id);
            if (app == null) {
                result.Data = false;
                return result;
            }
            // 看看数据库里是不是已经有对应的Track了
            AppTrack fromDb = DbSession.QueryOver<AppTrack>()
                .Where(t => t.App.Id == app.Id)
                .Where(t => t.User == CurrentUser.Id)
                .SingleOrDefault();

            // 数据库里没有，就保存一下
            if (fromDb == null) {
                fromDb = new AppTrack() {
                    User = CurrentUser.Id,
                    App = app,
                    Status = track.Status,
                    CreateTime = DateTime.Now,
                    CreatePrice = app.Price,
                    HasRead = true,
                    Rate = 0
                };
                // 如果是直接点了已经购买，就把Buy系列也补上
                if (fromDb.Status == AppTrackStatus.Owned) {
                    fromDb.BuyTime = fromDb.CreateTime;
                    fromDb.BuyPrice = fromDb.CreatePrice;
                }
                // 保存
                DbSession.Save(fromDb);
            }
            // 数据库里已经有了，更新，如果数据库里的状态已经和提交的一样了也不用更新，更新只能把状态更新到Owned
            else if (fromDb.Status != track.Status) {
                // CreateTime和CreatePrice肯定是有的
                fromDb.Status = track.Status;
                fromDb.HasRead = true;

                // 从InWish到Owned
                if (track.Status == AppTrackStatus.Owned) {
                    // 补上Buy系列
                    fromDb.BuyTime = DateTime.Now;
                    fromDb.BuyPrice = app.Price;
                }
                // 从Owned到InWish
                else {
                    // 去掉Buy系列
                    fromDb.BuyTime = null;
                    fromDb.BuyPrice = null;
                }

                DbSession.Update(fromDb);
            }


            result.Data = new { app = fromDb.App.Id, status = fromDb.Status };
            return result;
        }

        [HttpPost]
        [Transaction]
        public ActionResult Delete(int app) {
            JsonResult result = new JsonResult();
            result.ContentEncoding = Encoding.UTF8;
            result.ContentType = "application/json";

            AppTrack track = DbSession.QueryOver<AppTrack>()
                .Where(t => t.App.Id == app)
                .Where(t => t.User == CurrentUser.Id)
                .SingleOrDefault();

            if (track != null) {
                DbSession.Delete(track);
            }

            result.Data = true;
            return result;
        }

        [NonAction]
        private ActionResult List(TrackQuery query, AppTrackStatus status, int page) {
            IQueryOver<AppTrack, AppBrief> search = DbSession.QueryOver<AppTrack>()
                .Where(t => t.User == CurrentUser.Id)
                .Where(t => t.Status == status)
                .OrderBy(t => t.HasRead).Asc
                .JoinQueryOver(t => t.App);

            if (query.DeviceType != DeviceType.NotProvided) {
                search = search.Where(a => a.DeviceType == query.DeviceType || a.DeviceType == DeviceType.Universal);
            }
            if (!String.IsNullOrEmpty(query.Category)) {
                search = search.Where(a => a.PrimaryCategory == Category.Get(query.Category));
            }

            IQueryOverOrderBuilder<AppTrack, AppBrief> order = null;
            switch (query.Sort) {
                case AppSortType.Name:
                    order = search.OrderBy(a => a.Name);
                    break;
                case AppSortType.Price:
                    order = search.OrderBy(a => a.Price);
                    break;
                case AppSortType.Update:
                    order = search.OrderBy(a => a.LastValidUpdate.Time);
                    break;
                case AppSortType.Rating:
                    order = search.OrderBy(a => a.AverageUserRatingForCurrentVersion);
                    break;
                default:
                    break;
            }
            if (order != null) {
                if (query.Order == "desc") {
                    search = order.Desc;
                }
                else {
                    search = order.Asc;
                }
            }

            IList<AppTrack> list = search.Skip(query.StartIndex).Take(query.TakeSize).List();

            query.Fill(list);
            ViewBag.Tracks = list.ToDictionary(t => t.App.Id);

            TitleBuilder title = new TitleBuilder() {
                DeviceType = query.DeviceType,
                Category = Category.Get(query.Category)
            };
            ViewBag.Title = title.ForList(status == AppTrackStatus.InWish ? "我关注的应用" : "我购买的应用");
            ViewBag.Status = status;
            return View("List", query);
        }
    }
}
