using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog.Config;
using NLog.Targets;
using System.Security.Cryptography;
using System.Configuration;
using PingApp.Schedule.Task;
using System.Net.Mail;
using PingApp.Schedule.Storage;

namespace PingApp.Schedule {
    class Program {
        public static int BatchSize { get; private set; }

        public static bool Debug { get; private set; }

        static void Main(string[] args) {
            ActionType action = (ActionType)Enum.Parse(typeof(ActionType), Utility.Capitalize(args[0]));
            TaskNode[] tasks;
            IStorage input = null;

            switch (action) {
                case ActionType.Initialize:
                    tasks = new TaskNode[] {
                        new FullCatalogTask(),
                        new SearchApiTask(false),
                        new DbUpdateTask(DbCheckType.ForceInsert, false),
                        new IndexTask(false)
                    };
                    break;
                case ActionType.RssCheck:
                    tasks = new TaskNode[] {
                        new RssFeedCheckTask(),
                        new SearchApiTask(false),
                        new DbUpdateTask(DbCheckType.DiscardUpdate, false),
                        new IndexTask(true)
                    };
                    break;
                case ActionType.Update:
                    tasks = new TaskNode[] {
                        new GetAppHashTask(),
                        new SearchApiTask(true),
                        new DbUpdateTask(DbCheckType.CheckForUpdate, true),
                        new IndexTask(true),
                        //new NullTask()
                        new MailTask()
                    };
                    break;
                case ActionType.FullCheck:
                    tasks = new TaskNode[] {
                        new FullCatalogTask(),
                        new GetAppTask(true),
                        new SearchApiTask(false),
                        new DbUpdateTask(DbCheckType.DiscardUpdate, false),
                        new IndexTask(true)
                    };
                    break;
                case ActionType.TestSearch:
                    tasks = new TaskNode[] {
                        new TestSearchTask(args.Skip(1))
                    };
                    break;
                case ActionType.AddApp:
                    tasks = new TaskNode[] {
                        new SearchApiTask(false),
                        new DbUpdateTask(DbCheckType.ForceInsert, false),
                        new IndexTask(true)
                    };
                    input = new MemoryStorage();
                    input.Add(new int[] { Convert.ToInt32(args[1]) });
                    break;
                case ActionType.UpdateApp:
                    tasks = new TaskNode[] {
                        new GetAppHashTask(),
                        new SearchApiTask(true),
                        new DbUpdateTask(DbCheckType.CheckForUpdate, true),
                        new IndexTask(true)
                    };
                    input = new MemoryStorage();
                    input.Add(args.Skip(1).Select(s => Convert.ToInt32(s)));
                    break;
                case ActionType.RebuildIndex:
                    tasks = new TaskNode[] {
                        new GetAppTask(false),
                        new GetFullAppTask(),
                        new IndexTask(false)
                    };
                    break;
                case ActionType.Top100Check:
                    tasks = new TaskNode[] {
                        new Top100CheckTask()
                    };
                    break;
                default:
                    tasks = new TaskNode[0];
                    break;
            }


            if ((action == ActionType.Initialize || action == ActionType.RebuildIndex) && !Program.Debug) {
                Console.Write("To ensure this initialization, enter password: ");
                string password = Console.ReadLine();
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password + "PingApp"));
                password = BitConverter.ToString(bytes);
                if (password != "98-A5-F6-16-2A-A0-BE-71-AD-87-55-29-24-EF-FD-08") {
                    Console.WriteLine("Wrong password!");
                    return;
                }
            }

            string jobId = DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + action;
            string logRoot = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Log", jobId);
            TaskNode.Chain(action, logRoot, tasks);

            tasks[0].Run(input);

            // 发送错误
            if (File.Exists(Path.Combine(logRoot, "error.txt"))) {
                string error = File.ReadAllText(Path.Combine(logRoot, "error.txt"), Encoding.UTF8);
                using (SmtpClient client = new SmtpClient("localhost", 25)) {
                    MailMessage message = new MailMessage(
                        new MailAddress("administrator@pingapp.net", "PingApp.net Administrator"),
                        new MailAddress("pingapp@live.com")
                    );
                    message.SubjectEncoding = Encoding.UTF8;
                    message.Subject = String.Format("Schedule task failed {0:yyyy-MM-dd HH:mm}", DateTime.Now);
                    message.BodyEncoding = Encoding.UTF8;
                    message.Body = error + Environment.NewLine + "See " + logRoot + " for details";
                    try {
                        client.Send(message);
                    }
                    catch (Exception ex) {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(logRoot, "error.txt"), false, Encoding.UTF8)) {
                            writer.WriteLine();
                            writer.WriteLine();
                            writer.WriteLine();
                            writer.Write(ex.ToString());
                        }
                    }
                }
            }

            // 清理空间
            foreach (string directory in Directory.GetDirectories(logRoot)) {
                foreach (string subDirectory in Directory.GetDirectories(directory)) {
                    if (subDirectory.Contains("Output")) {
                        Directory.Delete(subDirectory, true);
                    }
                }
            }
        }

        static Program() {
            BatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchSize"]);
            Debug = Convert.ToBoolean(ConfigurationManager.AppSettings["Debug"]);
        }
    }
}
