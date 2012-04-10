using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using NLog;
using PingApp.Entity;
using PingApp.Repository;

namespace PingApp.Schedule.Infrastructure {
    sealed class UpdateNotifier : IDisposable {
        private static readonly Dictionary<AppUpdateType, string> templates;

        private static readonly Dictionary<AppUpdateType, string> subjects = new Dictionary<AppUpdateType, string>() {
            { AppUpdateType.NewRelease, "\"{0}\"版本更新了({1}->{2})" },
            { AppUpdateType.PriceDecrease, "\"{0}\"降价了(${1}->${2})" },
            { AppUpdateType.PriceFree, "\"{0}\"免费了" }
        };

        private readonly RepositoryEmitter repository;

        private readonly SmtpClient smtp;

        private readonly ProgramSettings settings;

        private readonly Logger logger;

        public UpdateNotifier(RepositoryEmitter repository, SmtpClient smtp, ProgramSettings settings, Logger logger) {
            this.repository = repository;
            this.smtp = smtp;
            this.settings = settings;
            this.logger = logger;
        }

        public void ProcessUpdate(App app, AppUpdate update) {
            /*
             * 1. 把所有AppTrack的HasRead改回false
             * 2. 发送邮件通知用户
             */

            int trackCount = repository.AppTrack.ResetReadStatusByApp(update.App);
            logger.Trace("Set {0} tracks to unread for app {1}-{2}", trackCount, app.Id, app.Brief.Name);

            SendMail(app, update);
        }

        private void SendMail(App app, AppUpdate update) {
            ICollection<AppTrack> tracks = repository.AppTrack.RetrieveByApp(app.Id);
            foreach (AppTrack track in tracks) {
                User user = repository.User.Retrieve(track.User);
                if (track.RequireNotification(user, update.Type)) {
                    MailMessage message = CreateMailMessage(user, app, update);

                    if (settings.Debug) {
                        StringBuilder text = new StringBuilder()
                            .AppendLine("Send mail:")
                            .AppendLine("From: " + message.From.Address)
                            .AppendLine("To: " + message.To[0].Address)
                            .AppendLine("Subject: " + message.Subject)
                            .AppendLine(message.Body);
                        logger.Trace(text);
                    }
                    else {
                        try {
                            smtp.Send(message);
                            logger.Trace(
                                "Sent mail to user {0}-{1} via {2} for update{3}",
                                user.Id, user.Username, user.Email, update.Id
                            );
                        }
                        catch (SmtpFailedRecipientException) {
                            logger.Warn("Failed to send email to {0}, address may be invalid", user.Email);
                        }
                        catch (Exception ex) {
                            logger.ErrorException("Encountered unexpected exception when send email", ex);
                        }
                    }
                }
            }
        }

        private MailMessage CreateMailMessage(User user, App app, AppUpdate update) {
            MailMessage message = new MailMessage(
                new MailAddress(settings.MailAddress, settings.MailUser, Encoding.UTF8),
                new MailAddress(user.Email)
            );
            message.SubjectEncoding = Encoding.UTF8;
            message.Subject = String.Format(subjects[update.Type], app.Brief.Name, update.OldValue, update.NewValue);
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            message.Body = String.Format(
                templates[update.Type],
                user.Username,
                app.Brief.Name,
                update.OldValue,
                update.NewValue,
                app.Id,
                app.Brief.ViewUrl,
                DateTime.Now,
                app.ReleaseNotes
            );

            return message;
        }

        static UpdateNotifier() {
            // 初始化模板
            IEnumerable<AppUpdateType> values = new AppUpdateType[] { 
                AppUpdateType.NewRelease, 
                AppUpdateType.PriceDecrease, 
                AppUpdateType.PriceFree
            };

            string templateDirectory = 
                Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "MailTemplate");
            templates = 
                values.ToDictionary(t => t, t => File.ReadAllText(Path.Combine(templateDirectory, t + ".htm")));
        }

        public void Dispose() {
            try {
                smtp.Dispose();
            }
            catch (InvalidOperationException) {
            }

            logger.Info("Disposed update notifier");
        }
    }
}
