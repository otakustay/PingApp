using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    class Top100CheckModule : NinjectModule {
        public override string Name {
            get {
                return ActionType.Top100Check.ToString();
            }
        }

        public override void Load() {
            Bind<Top100CheckTask>().ToSelf().Named(Name);

            TaskNode[] tasks = new TaskNode[] {
                Kernel.Get<Top100CheckTask>(Name)
            };
            Bind<TaskNode[]>().ToConstant(tasks).Named(Name);
        }
    }
}
