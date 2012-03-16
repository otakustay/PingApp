using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.IO;

namespace PingApp.Schedule {
    abstract class TaskNode {
        public string LogRoot { get; set; }

        public Logger Log { get; private set; }

        public ActionType Action { get; private set; }

        public TaskNode NextTask { get; set; }

        public void Run(IStorage input) {
            Log = Utility.GetLogger(LogRoot, Name);
            IStorage output = RunTask(input);

            if (NextTask != null) {
                NextTask.Run(output);
            }
        }

        protected virtual string Name {
            get {
                return GetType().Name;
            }
        }

        protected abstract IStorage RunTask(IStorage input);

        public static void Chain(ActionType action, string logRoot, TaskNode[] tasks) {
            for (int i = 0; i < tasks.Length; i++) {
                TaskNode task = tasks[i];
                task.Action = action;
                task.LogRoot = Path.Combine(logRoot, task.Name);
                if (i < tasks.Length - 1) {
                    task.NextTask = tasks[i + 1];
                }
            }
        }
    }
}
