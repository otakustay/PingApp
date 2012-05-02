using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ninject;
using NLog;
using NLog.Config;
using NLog.Targets;
using PanGu.Match;
using PingApp.Infrastructure;
using PingApp.Infrastructure.Default.Dependency;
using PingApp.Repository;
using PingApp.Repository.Mongo.Dependency;
using PingApp.Schedule.Dependency;
using PingApp.Schedule.Task;
using LogLevel = NLog.LogLevel;

namespace PingApp.Schedule {
    class Program {
        static void Main(string[] args) {
            ActionType action = (ActionType)Enum.Parse(typeof(ActionType), args[0].Captalize());
            IKernel kernel = new StandardKernel();
            kernel.Load(new SharedModule(action));
            kernel.Load(new MongoRepositoryModule());
            kernel.Load(new InfrastructureModule());
            kernel.Load(new InitializeModule());
            kernel.Load(new RssCheckModule());
            kernel.Load(new UpdateModule());
            kernel.Load(new CheckNewModule());
            kernel.Load(new RescueModule());

            ConfigureLogger(action, kernel);

            AppDomain.CurrentDomain.ProcessExit += (target, e) => LogManager.Configuration = null;

            Type taskType = Type.GetType("PingApp.Schedule.Task." + action + "Task");
            TaskBase task = kernel.Get(taskType) as TaskBase;

            if (task == null) {
                Console.WriteLine("No task for action {0} defined", args[0]);
                return;
            }

            using (task) {
                string[] taskArguments = new string[args.Length - 1];
                Array.Copy(args, 1, taskArguments, 0, taskArguments.Length);
                task.Run(taskArguments);
            }

            // NLog在mono平台上有BUG，会导致程序无法退出，始终等待Logger进行Flush，这行代码强制完成Flush
            LogManager.Configuration = null;
        }

        private static void ConfigureLogger(ActionType action, IKernel kernel) {
            string logRoot = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                "Log",
                DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + action
            );
            ProgramSettings settings = kernel.Get<ProgramSettings>();
            string layout = "${longdate}|${level}|${logger}|${message}${onexception:inner=${newline}}${exception:format=tostring}";

            LoggingConfiguration config = new LoggingConfiguration();

            ConsoleTarget console = new ConsoleTarget();
            config.AddTarget("console", console);
            FileTarget file = new FileTarget();
            config.AddTarget("file", file);
            FileTarget debug = new FileTarget();
            config.AddTarget("debug", debug);
            FileTarget trace = new FileTarget();
            config.AddTarget("trace", trace);
            FileTarget error = new FileTarget();
            config.AddTarget("error", error);

            console.Layout = settings.Debug ? "${longdate}|${level}|${logger}|${message}" : layout;
            file.FileName = logRoot + "/log.txt";
            file.Encoding = Encoding.UTF8;
            file.Layout = layout;
            debug.FileName = logRoot + "/debug.txt";
            debug.Encoding = Encoding.UTF8;
            debug.Layout = layout;
            trace.FileName = logRoot + "/verbose.txt";
            trace.Encoding = Encoding.UTF8;
            trace.Layout = layout;
            error.FileName = logRoot + "/error.txt";
            error.Encoding = Encoding.UTF8;
            error.Layout = layout;

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, console));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, file));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debug));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, trace));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, error));

            LogManager.Configuration = config;
        }
    }
}
