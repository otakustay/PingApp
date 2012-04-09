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
using Ninject;
using System.Collections;
using PingApp.Schedule.Dependency;
using PingApp.Repository;
using PingApp.Schedule.Infrastructure;
using NLog;
using PingApp.Repository.Mongo.Dependency;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PanGu.Match;
using System.Diagnostics;

namespace PingApp.Schedule {
    class Program {
        static void Main(string[] args) {
            ActionType action = (ActionType)Enum.Parse(typeof(ActionType), args[0].Captalize());
            IKernel kernel = new StandardKernel();
            kernel.Load(new MongoRepositoryModule());
            kernel.Load(new SharedModule(action));
            kernel.Load(new InitializeModule());
            kernel.Load(new UpdateModule());

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

        }
    }
}
