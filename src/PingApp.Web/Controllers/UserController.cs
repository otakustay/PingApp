using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PingApp.Entity;
using PingApp.Web.Infrastructures;
using NHibernate.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using NHibernate.Criterion;
using PingApp.Web.Models;
using System.Net.Mail;
using System.IO;

namespace PingApp.Web.Controllers {
    public class UserController : BaseController {
        private static readonly string salt = "PingApp.net";

        [HttpGet]
        public ActionResult Register() {
            ViewBag.Title = "注册";
            return View(new User());
        }

        [HttpPost]
        [Transaction]
        public ActionResult Save(User user) {
            user.NotifyOnWishFree = true;
            user.NotifyOnWishPriceDrop = true;
            user.Password = HashPassword(user.Password);
            user.PreferredLanguagePriority = 1000;
            user.RegisterTime = DateTime.Now;

            bool exists = DbSession.Query<User>().Any(u => u.Email == user.Email || u.Username == user.Username);
            if (exists) {
                // 处理重复
                ViewBag.Message = "该电子邮件或用户名已经被注册了，请重新选择";
                ViewBag.Title = "修改资料";
                return View(user);
            }
            else {
                user.Id = (int)DbSession.Save(user);
                SetAsAuthenticated(user, false);
                return RedirectToAction("ImportGuide", "User");
            }
        }

        [HttpGet]
        public ActionResult SignIn() {
            ViewBag.Title = "登录";
            ViewBag.Remember = false;
            ViewBag.Email = String.Empty;
            return View();
        }

        [HttpPost]
        public ActionResult Authenticate(string email, string password, bool remember = false) {
            password = HashPassword(password);
            User user = DbSession.QueryOver<User>().Where(u => u.Email == email).SingleOrDefault();

            // 登录成功
            if (user != null && user.Password == password) {
                SetAsAuthenticated(user, remember);

                string returnUrl = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["ReturnUrl"];
                if ((user.Status & UserStatus.PasswordReset) == UserStatus.PasswordReset) {
                    // 重设了密码没改的，跳到资料页面要求修改
                    return RedirectToAction("Profile", "User");
                }
                else if (String.IsNullOrEmpty(returnUrl)) {
                    return RedirectToAction("Index", "Home");
                }
                else {
                    return Redirect(returnUrl);
                }
            }
            else {
                ViewBag.Title = "登录";
                ViewBag.ErrorMessage = "登录失败，请检查您填写的电子邮箱和密码是否正确并再次登录";
                ViewBag.Email = email;
                ViewBag.Remember = remember;
                return View("SignIn", new User() { Email = email });
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult SignOut() {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Profile() {
            ViewBag.Title = "修改资料";
            User user = DbSession.Get<User>(Convert.ToInt32(User.Identity.Name));
            return View(user);
        }

        [HttpPost]
        [Transaction]
        [Authorize]
        public ActionResult UpdateProfile(User user, string oldPassword, string newPassword) {
            ViewBag.Title = "修改资料";
            User origin = DbSession.Get<User>(Convert.ToInt32(User.Identity.Name));
            origin.Description = user.Description ?? String.Empty;
            origin.Website = user.Website ?? String.Empty;
            origin.NotifyOnOwnedUpdate = user.NotifyOnOwnedUpdate;
            origin.NotifyOnWishFree = user.NotifyOnWishFree;
            origin.NotifyOnWishPriceDrop = user.NotifyOnWishPriceDrop;
            origin.NotifyOnWishUpdate = user.NotifyOnWishUpdate;
            origin.PreferredLanguagePriority = user.PreferredLanguagePriority;

            // 修改密码的话再验证一次
            if (!String.IsNullOrEmpty(oldPassword)) {
                if (origin.Password != HashPassword(oldPassword)) {
                    ViewBag.Message = "原密码错误，请检查后重新提交";
                    return View("Profile", origin);
                }
                origin.Password = HashPassword(newPassword);
                origin.Status &= ~UserStatus.PasswordReset;
            }

            DbSession.Update(origin);
            Session["CurrentUser"] = origin;

            ViewBag.Message = "更新资料成功";
            return View("Profile", origin);
        }

        [HttpGet]
        public ActionResult ResetPassword() {
            ViewBag.Title = "重设密码";
            return View();
        }

        [HttpPost]
        [Transaction]
        public ActionResult ResetPassword(string email) {
            ViewBag.Title = "重设密码";

            User user = DbSession.QueryOver<User>()
                .Where(u => u.Email == email)
                .SingleOrDefault();

            if (user == null) {
                ViewBag.Message = "不存在该电子邮件指定的用户，请确认后重试";
                return View();
            }

            Random random = new Random();
            string charset = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()_+-={}[]<>,.?/~";
            string password = String.Empty;
            for (int i = 0; i < 12; i++) {
                password += charset[random.Next(charset.Length)];
            }
            user.Password = HashPassword(password);
            user.Status |= UserStatus.PasswordReset;
            DbSession.Update(user);

            using (SmtpClient client = new SmtpClient("localhost", 25)) {
                string template = System.IO.File.ReadAllText(Server.MapPath("~/MailTemplate/PasswordReset.htm"), Encoding.UTF8);

                MailMessage message = new MailMessage(
                    new MailAddress("notification@pingapp.net", "PingApp.net"),
                    new MailAddress(email)
                );
                message.SubjectEncoding = Encoding.UTF8;
                message.Subject = "PingApp.net密码重设通知";
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;
                message.Body = String.Format(template, user.Username, password, DateTime.Now);

                client.Send(message);
            }

            return View("PasswordResetOk", (object)email);
        }

        #region 购买记录导入

        [HttpGet]
        [Authorize]
        public ActionResult ImportGuide() {
            // 只允许导入一次
            if ((CurrentUser.Status & UserStatus.AppImported) == UserStatus.AppImported) {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Title = "导入应用购买历史";
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Import() {
            // 只允许导入一次
            if ((CurrentUser.Status & UserStatus.AppImported) == UserStatus.AppImported) {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Title = "导入应用购买历史";
            return View();
        }

        [HttpPost]
        [Transaction]
        [Authorize]
        public ActionResult Import(string history) {
            // 只允许导入一次
            if ((CurrentUser.Status & UserStatus.AppImported) == UserStatus.AppImported) {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Title = "导入应用购买历史";
            history = history.Trim();
            if (history.Length == 0) {
                return View("Import");
            }

            string[] lines = history.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> appNames = new List<string>();
            foreach (string line in lines.Select(s => s.Trim())) {
                if (line == "list.txt") {
                    continue;
                }

                int index = line.LastIndexOf('.');
                if (index < 0) {
                    ViewBag.Message = "提供的信息格式有误，请仔细检查后重新操作";
                    ViewBag.Value = history;
                    return View("Import");
                }
                string name = line.Substring(0, index);
                string extension = line.Substring(index + 1);
                if (extension.ToLower() != "ipa") {
                    ViewBag.Message = "提供的信息格式有误，请仔细检查后重新操作";
                    ViewBag.Value = history;
                    return View("Import");
                }
                // 分解名称
                index = name.LastIndexOf(' ');
                if (index < 0) {
                    ViewBag.Message = "提供的信息格式有误，请仔细检查后重新操作";
                    ViewBag.Value = history;
                    return View("Import");
                }
                appNames.Add(name.Substring(0, index));
            }

            // 提取应用
            IList<AppBrief> apps = DbSession.QueryOver<AppBrief>()
                .Where(Restrictions.InG<string>("Name", appNames))
                .List();
            ICollection<string> notFound = appNames.Except(apps.Select(a => a.Name)).ToArray();

            ImportModel model = new ImportModel();
            model.Apps = apps;
            model.NotFound = notFound;

            return View("ConfirmImport", model);
        }

        [HttpPost]
        [Transaction]
        [Authorize]
        public ActionResult SaveImport(int[] apps) {
            // 只允许导入一次
            if ((CurrentUser.Status & UserStatus.AppImported) == UserStatus.AppImported) {
                return RedirectToAction("Index", "Home");
            }
            // 找到已经关注过的
            IEnumerable<int> existed = DbSession.QueryOver<AppTrack>()
                .Where(t => t.User == CurrentUser.Id)
                .Where(Restrictions.InG<int>("App.Id", apps))
                .List()
                .Select(t => t.App.Id);

            // 从数据库中取出需要关注的应用
            IEnumerable<AppBrief> insert = DbSession.QueryOver<AppBrief>()
                .Where(Restrictions.InG<int>("Id", apps.Except(existed)))
                .List();

            // 生成AppTrack插入
            foreach (AppBrief app in insert) {
                AppTrack track = new AppTrack() {
                    App = app,
                    User = CurrentUser.Id,
                    BuyPrice = app.Price,
                    BuyTime = DateTime.Now,
                    CreatePrice = app.Price,
                    CreateTime = DateTime.Now,
                    HasRead = true,
                    Status = AppTrackStatus.Owned
                };
                DbSession.Save(track);
            }

            // 更新用户，以后不需要导入了
            User user = DbSession.Get<User>(CurrentUser.Id);
            user.Status |= UserStatus.AppImported;
            DbSession.Update(user);
            Session["CurrentUser"] = user;

            return RedirectToAction("OwnedList", "Track");
        }

        [HttpPost]
        [Transaction]
        [Authorize]
        public ActionResult CancelImport() {
            // 只允许导入一次
            if ((CurrentUser.Status & UserStatus.AppImported) == UserStatus.AppImported) {
                return RedirectToAction("Index", "Home");
            }
            User user = DbSession.Get<User>(CurrentUser.Id);
            user.Status |= UserStatus.AppImported;
            DbSession.Update(user);
            Session["CurrentUser"] = user;
            return new EmptyResult();
        }

        #endregion

        private void SetAsAuthenticated(User user, bool remember) {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, user.Id.ToString(), DateTime.Now, DateTime.MaxValue, remember, String.Empty);
            HttpCookie cookie = new HttpCookie(
                FormsAuthentication.FormsCookieName,
                FormsAuthentication.Encrypt(ticket)
            );
            if (remember) {
                cookie.Expires = ticket.Expiration;
            }
            Response.Cookies.Add(cookie);
            Session["CurrentUser"] = user;
        }

        private static string HashPassword(string s) {
            s += salt;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(s));
            string encrypted = BitConverter.ToString(bytes).Replace("-", String.Empty);
            return encrypted;
        }
    }
}
