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
using PingApp.Repository;
using PingApp.Repository.Mongo.Dependency;
using PingApp.Schedule.Dependency;
using PingApp.Schedule.Infrastructure;
using PingApp.Schedule.Task;

namespace PingApp.Schedule {
    class Program {
        static void Main(string[] args) {
            ActionType action = (ActionType)Enum.Parse(typeof(ActionType), args[0].Captalize());
            IKernel kernel = new StandardKernel();
            kernel.Load(new MongoRepositoryModule());
            kernel.Load(new SharedModule(action));
            kernel.Load(new InitializeModule());
            kernel.Load(new RssCheckModule());
            kernel.Load(new UpdateModule());
            kernel.Load(new CheckNewModule());
            kernel.Load(new RescueModule());

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
    }
}
