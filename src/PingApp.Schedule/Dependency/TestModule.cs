using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    class TestModule : NinjectModule {
        public override string Name {
            get {
                return ActionType.Test.ToString();
            }
        }

        public override void Load() {
            Bind<TestTask>().ToSelf().Named(Name);

            TaskNode[] tasks = new TaskNode[] {
                Kernel.Get<TestTask>(Name)
            };
            Bind<TaskNode[]>().ToConstant(tasks).Named(Name);
        }
    }
}
