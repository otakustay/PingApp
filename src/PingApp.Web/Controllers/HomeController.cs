using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PingApp.Web.Infrastructures;
using PingApp.Entity;
using PingApp.Web.Models;
using NHibernate;
using SimpleLucene;
using System.IO;
using PingApp.Utility.Lucene;
using System.Configuration;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NHibernate.Criterion;
using System.Text.RegularExpressions;

namespace PingApp.Web.Controllers {
    public class HomeController : BaseController {
        private static readonly Regex byApp = new Regex(@"^#\d+$", RegexOptions.Compiled);

        private static readonly Regex byDeveloper = new Regex(@"^\.\d+$", RegexOptions.Compiled);

        [HttpGet]
        [Transaction]
        [Region("Home")]
        public ActionResult Index(ListQuery query, int page = 1) {
            TitleBuilder title = new TitleBuilder();

            IQueryOver<AppBrief, AppBrief> search = DbSession.QueryOver<AppBrief>();
            int languagePriority = User.Identity.IsAuthenticated ?
                CurrentUser.PreferredLanguagePriority : Default.LanguagePriority;
            search = search.Where(a => a.LanguagePriority >= languagePriority);
            search = search.Where(a => a.IsActive);

            if (query.DeviceType != DeviceType.NotProvided) {
                search = search.Where(a => a.DeviceType == query.DeviceType || a.DeviceType == DeviceType.Universal);
                title.DeviceType = query.DeviceType;
            }
            if (!String.IsNullOrEmpty(query.Category)) {
                search = search.Where(a => a.PrimaryCategory == Category.Get(query.Category));
                title.Category = Category.Get(query.Category);
            }
            if (query.PriceMode == PriceMode.Free) {
                search = search.Where(a => a.Price == 0);
                title.PriceMode = query.PriceMode;
            }
            else if (query.PriceMode == PriceMode.Paid) {
                search = search.Where(a => a.Price > 0);
                title.PriceMode = query.PriceMode;
            }
            if (query.UpdateType.HasValue) {
                search = search.Where(a => a.LastValidUpdate.Type == query.UpdateType.Value);
                title.UpdateType = query.UpdateType.Value;
            }
            search = search.OrderBy(a => a.LastValidUpdate.Time).Desc;

            IList<AppBrief> apps = search.Skip(query.StartIndex).Take(query.TakeSize).List();
            query.Fill(apps);

            if (User.Identity.IsAuthenticated) {
                Dictionary<int, AppTrack> tracks = DbSession.QueryOver<AppTrack>()
                    .Where(t => t.User == CurrentUser.Id)
                    .Where(Restrictions.InG<int>("App.Id", apps.Take(query.PageSize).Select(a => a.Id)))
                    .List()
                    .ToDictionary(t => t.App.Id);
                ViewBag.Tracks = tracks;
            }
            else {
                ViewBag.Tracks = new Dictionary<int, AppTrack>();
            }

            ViewBag.Title = title.ForList("最近更新应用");
            return View(query);
        }

        [HttpGet]
        [Transaction]
        [Region("Home")]
        public ActionResult Search(SearchQuery query, int page = 1) {
            // 按应用搜
            if (byApp.IsMatch(query.Keywords)) {
                return Detail(Convert.ToInt32(query.Keywords.Substring(1)));
            }
            // 按开发者搜
            else if (byDeveloper.IsMatch(query.Keywords)) {
                return ByDeveloper(new Developer() { Id = Convert.ToInt32(query.Keywords.Substring(1)) });
            }

            if (String.IsNullOrEmpty(query.Order)) {
                if (query.Sort == AppSortType.Rating || query.Sort == AppSortType.Update) {
                    query.Order = "desc";
                }
            }

            Category category = Category.Get(query.Category);
            DirectoryInfo directory = new DirectoryInfo(ConfigurationManager.AppSettings["LuceneIndexDirectory"]);
            AppQuery search = new AppQuery()
                .WithCategory(category == null ? 0 : category.Id)
                .WithDeviceType(query.DeviceType)
                .WithKeywords(query.Keywords)
                //.WithLanguagePriority(User.Identity.IsAuthenticated ? CurrentUser.PreferredLanguagePriority : 1000)
                .SortBy(query.Sort, query.Order == "desc");

            Query q = search.Query;
            q.ToString();

            IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(directory), true);
            TopDocs docs = searcher.Search(search.Query, null, 5000, search.Sort);
            int[] found = docs.scoreDocs
                .Skip(query.StartIndex)
                .Take(query.TakeSize)
                .Select(d => searcher.Doc(d.doc).GetField("Id").StringValue())
                .Select(s => Convert.ToInt32(s))
                .ToArray();

            searcher.Close();

            List<AppBrief> list = DbSession.QueryOver<AppBrief>()
                .Where(Restrictions.InG("Id", found))
                .List()
                .ToList();
            list.Sort((x, y) => (Array.IndexOf<int>(found, x.Id) - Array.IndexOf<int>(found, y.Id)));
            query.Fill(list);

            if (User.Identity.IsAuthenticated) {
                Dictionary<int, AppTrack> tracks = DbSession.QueryOver<AppTrack>()
                    .Where(t => t.User == CurrentUser.Id)
                    .Where(Restrictions.InG<int>("App.Id", list.Take(query.PageSize).Select(a => a.Id)))
                    .List()
                    .ToDictionary(t => t.App.Id);
                ViewBag.Tracks = tracks;
            }
            else {
                ViewBag.Tracks = new Dictionary<int, AppTrack>();
            }

            TitleBuilder title = new TitleBuilder() {
                Keywords = query.Keywords,
                Category = Category.Get(query.Category),
                DeviceType = query.DeviceType,
            };
            ViewBag.Title = title.ForList("搜索");
            ViewBag.Keywords = query.Keywords;
            return View(query);
        }

        [HttpGet]
        [Transaction]
        [Region("Home")]
        public ActionResult ByDeveloper(Developer developer, int page = 1) {
            PagedQuery<AppBrief> query = new PagedQuery<AppBrief>() { Page = page };
            IList<AppBrief> apps = DbSession.QueryOver<AppBrief>()
                .Where(a => a.Developer.Id == developer.Id)
                .OrderBy(a => a.LastValidUpdate.Time).Desc
                .Skip(query.StartIndex)
                .Take(query.TakeSize)
                .List();
            query.Fill(apps);

            if (apps.Count > 0) {
                developer = apps[0].Developer;

                if (User.Identity.IsAuthenticated) {
                    Dictionary<int, AppTrack> tracks = DbSession.QueryOver<AppTrack>()
                        .Where(t => t.User == CurrentUser.Id)
                        .Where(Restrictions.InG<int>("App.Id", apps.Take(query.PageSize).Select(a => a.Id)))
                        .List()
                        .ToDictionary(t => t.App.Id);
                    ViewBag.Tracks = tracks;
                }
                else {
                    ViewBag.Tracks = new Dictionary<int, AppTrack>();
                }
            }

            ViewBag.Title = String.Format(
                "{0}的所有应用", 
                String.IsNullOrEmpty(developer.Name) ? "开发者" + developer.Id : developer.Name
            );
            ViewBag.Developer = developer;
            return View("PlainList", query);
        }

        [HttpGet]
        [Transaction]
        public ActionResult Detail(int id) {
            DetailModel model = new DetailModel();

            App app = DbSession.Get<App>(id);

            if (app == null) {
                return new HttpNotFoundResult();
            }

            model.App = app;
            if (User.Identity.IsAuthenticated) {
                AppTrack track = DbSession.QueryOver<AppTrack>()
                    .Where(t => t.User == CurrentUser.Id)
                    .Where(t => t.App.Id == id)
                    .SingleOrDefault();
                if (track != null) {
                    model.Owned = track.Status == AppTrackStatus.Owned;
                    model.InWish = track.Status == AppTrackStatus.InWish;
                }
            }
            // 所有更新
            IEnumerable<AppUpdate> updates = DbSession.QueryOver<AppUpdate>()
                .Where(u => u.App == app.Id)
                .Where(u => u.Type != AppUpdateType.AddToNote)
                .OrderBy(u => u.Time).Desc
                .List();
            model.Updates = updates;

            // 同作者应用
            IEnumerable<AppBrief> relativeApps = DbSession.QueryOver<AppBrief>()
                .Where(a => a.Developer.Id == app.Brief.Developer.Id)
                .OrderBy(a => a.AverageUserRatingForCurrentVersion).Desc
                .Take(17)
                .List()
                .Where(a => a.Id != app.Id)
                .Take(16);
            model.RelativeApps = relativeApps;

            TitleBuilder title = new TitleBuilder() {
                AppName = app.Brief.Name,
                Category = app.Brief.PrimaryCategory,
                DeviceType = app.Brief.DeviceType,
                Price = app.Brief.Price
            };
            ViewBag.Title = title.ForDetail();
            return View("Detail", model);
        }
    }
}
