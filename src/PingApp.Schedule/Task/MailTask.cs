using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using MySql.Data.MySqlClient;
using PingApp.Utility;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Diagnostics;

namespace PingApp.Schedule.Task {
    class MailTask : TaskNode {
        private static readonly string templateDirectory =
            Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "MailTemplate");

        private static readonly Dictionary<AppUpdateType, string> templates;

        private static readonly Dictionary<AppUpdateType, string> subjects = new Dictionary<AppUpdateType, string>() {
            { AppUpdateType.NewRelease, "\"{0}\"版本更新了({1}->{2})" },
            { AppUpdateType.PriceDecrease, "\"{0}\"降价了(${1}->${2})" },
            { AppUpdateType.PriceFree, "\"{0}\"免费了" }
        };

        private static readonly Dictionary<int, User> users = GetAllUsers();

        static MailTask() {
            // 初始化模板
            IEnumerable<AppUpdateType> values = new AppUpdateType[] { 
                AppUpdateType.NewRelease, AppUpdateType.PriceDecrease, AppUpdateType.PriceFree };
            templates = values.ToDictionary(
                t => t,
                t => File.ReadAllText(Path.Combine(templateDirectory, t.ToString() + ".htm"))
            );
        }

        protected override IStorage RunTask(IStorage input) {
            IEnumerable<AppUpdate> updates = input.Get<IEnumerable<AppUpdate>>();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Log.Info("Get all related apps from db, expecting {0} apps", updates.Count());

            Dictionary<int, AppBrief> apps = GetApps(updates.Select(u => u.App));

            watch.Stop();
            Log.Info("{0} apps retrieved from db using {1}ms", apps.Count, watch.ElapsedMilliseconds);

            watch.Start();

            if (Program.Debug) {
                foreach (AppUpdate update in updates) {
                    SendMail(apps[update.App], update);
                }
            }
            else {
                updates.AsParallel().ForAll(u => SendMail(apps[u.App], u));
            }

            watch.Stop();
            Log.Info("Work done using {0}ms", watch.ElapsedMilliseconds);

            return null;
        }

        private void SendMail(AppBrief app, AppUpdate update) {
            List<AppTrack> tracks = GetTracks(app.Id);

            Log.Debug("Start send mail for --type={0}, --app={1}, --app-name={2}", update.Type, app.Id, app.Name);
            using (SmtpClient client = new SmtpClient("localhost", 25)) {
                foreach (AppTrack track in tracks) {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    User user = users[track.User];

                    if (track.RequireNotification(user, update.Type)) {

                        MailMessage message = new MailMessage(
                            new MailAddress("notification@pingapp.net", "PingApp.net", Encoding.UTF8),
                            new MailAddress(user.Email)
                        );
                        message.SubjectEncoding = Encoding.UTF8;
                        message.Subject = String.Format(subjects[update.Type], app.Name, update.OldValue, update.NewValue);
                        message.BodyEncoding = Encoding.UTF8;
                        message.IsBodyHtml = true;
                        message.Body = String.Format(
                            templates[update.Type],
                            user.Username,
                            app.Name,
                            update.OldValue,
                            update.NewValue,
                            app.Id,
                            app.ViewUrl,
                            DateTime.Now,
                            app.ReleaseNotes
                        );

                        if (Program.Debug) {
                            StringBuilder text = new StringBuilder()
                                .AppendLine("From: " + message.From.Address)
                                .AppendLine("To: " + message.To[0].Address)
                                .AppendLine("Subject: " + message.Subject)
                                .AppendLine(message.Body);
                            Log.Trace(text);
                        }
                        else {
                            try {
                                client.Send(message);
                                watch.Stop();
                                Log.Debug(
                                    "Mail sent to {0} with --type={1}, --app={2}, --app-name={3} using {4}ms",
                                    user.Email, update.Type, app.Id, app.Name, watch.ElapsedMilliseconds
                                );
                            }
                            catch (SmtpFailedRecipientsException) {
                                Log.Warn("Cannot send mail to {0}", user.Email);
                            }
                            catch (Exception ex) {
                                Log.ErrorException("Mail send failed", ex);
                            }
                        }
                    }
                    else {
                    }
                }
            }
        }

        private List<AppTrack> GetTracks(int app) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Log.Info("Start get track infos from db for app {0}", app);

            List<AppTrack> tracks = new List<AppTrack>();
            using (MySqlConnection connection = new MySqlConnection(
                ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                MySqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from AppTrack where App = ?App";
                cmd.Parameters.AddWithValue("?App", app);
                using (IDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        AppTrack track = new AppTrack() {
                            Id = reader.Get<int>("Id"),
                            App = new AppBrief() { Id = reader.Get<int>("App") },
                            User = reader.Get<int>("User"),
                            Status = reader.Get<AppTrackStatus>("Status")
                        };
                        tracks.Add(track);
                    }
                }
            }

            watch.Stop();
            Log.Info("{0} tracks retrieved from db for app {1} using {2}ms", tracks.Count, app, watch.ElapsedMilliseconds);

            return tracks;
        }

        private Dictionary<int, AppBrief> GetApps(IEnumerable<int> list) {
            Dictionary<int, AppBrief> apps = new Dictionary<int, AppBrief>();
            string sql = "select * from AppBrief where Id in ({0})";

            using (MySqlConnection connection = new MySqlConnection(
                ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                foreach (IEnumerable<int> part in Utility.Partition(list, 100)) {
                    MySqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format(sql, String.Join(",", list));
                    using (IDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            AppBrief app = new AppBrief() {
                                Id = reader.Get<int>("Id"),
                                Name = reader.Get<string>("Name"),
                                Version = reader.Get<string>("Version"),
                                ReleaseDate = reader.Get<DateTime>("ReleaseDate"),
                                Introduction = reader.Get<string>("Introduction"),
                                ReleaseNotes = reader.Get<string>("ReleaseNotes"),
                                Price = reader.Get<float>("Price"),
                                Currency = reader.Get<string>("Currency"),
                                FileSize = reader.Get<int>("FileSize"),
                                ViewUrl = reader.Get<string>("ViewUrl"),
                                IconUrl = reader.Get<string>("IconUrl"),
                                AverageUserRatingForCurrentVersion = reader.Get<float?>("AverageUserRatingForCurrentVersion"),
                                UserRatingCountForCurrentVersion = reader.Get<int?>("UserRatingCountForCurrentVersion"),
                                LanguagePriority = reader.Get<int>("LanguagePriority"),
                                Features = reader.Get<string>("Features")
                                    .Split(',').Where(s => s.Length > 0).ToArray(),
                                SupportedDevices = reader.Get<string>("SupportedDevices")
                                    .Split(',').Where(s => s.Length > 0).ToArray(),
                                PrimaryCategory = Category.Get(reader.Get<int>("PrimaryCategory")),
                                Developer = new Developer() {
                                    Id = reader.Get<int>("DeveloperId"),
                                    Name = reader.Get<string>("DeveloperName"),
                                    ViewUrl = reader.Get<string>("DeveloperViewUrl")
                                },
                                LastValidUpdate = new AppUpdate() {
                                    Time = reader.Get<DateTime>("LastValidUpdateTime"),
                                    Type = reader.Get<AppUpdateType>("LastValidUpdateType"),
                                    OldValue = reader.Get<string>("LastValidUpdateOldValue"),
                                    NewValue = reader.Get<string>("LastValidUpdateNewValue"),
                                    App = reader.Get<int>("Id")
                                }
                            };
                            apps[app.Id] = app;
                        }
                    }
                }
            }

            Log.Warn("Not found: {0}", String.Join(",", list.Except(apps.Keys)));

            return apps;
        }

        private static Dictionary<int, User> GetAllUsers() {
            Dictionary<int, User> users = new Dictionary<int, User>();
            using (MySqlConnection connection = new MySqlConnection(
                ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                MySqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from User";
                using (IDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        User user = new User() {
                            Id = reader.Get<int>("Id"),
                            Username = reader.Get<string>("Username"),
                            Email = reader.Get<string>("Email"),
                            NotifyOnOwnedUpdate = reader.Get<bool>("NotifyOnOwnedUpdate"),
                            NotifyOnWishUpdate = reader.Get<bool>("NotifyOnWishUpdate"),
                            NotifyOnWishFree = reader.Get<bool>("NotifyOnWishFree"),
                            NotifyOnWishPriceDrop = reader.Get<bool>("NotifyOnWishPriceDrop")
                        };
                        users[user.Id] = user;
                    }
                }
            }
            return users;
        }
    }
}
