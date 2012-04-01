using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    class RebuildIndexModule : NinjectModule {
        public override string Name {
            get {
                return ActionType.RebuildIndex.ToString();
            }
        }

        public override void Load() {
            Bind<GetAppTask>().ToSelf().Named(Name)
                .WithConstructorArgument("computeDiff", false);
            Bind<GetFullAppTask>().ToSelf().Named(Name);
            Bind<IndexTask>().ToSelf().Named(Name)
                .WithConstructorArgument("incremental", false);

            TaskNode[] tasks = new TaskNode[] {
                Kernel.Get<GetAppTask>(Name),
                Kernel.Get<GetFullAppTask>(Name),
                Kernel.Get<IndexTask>(Name)
            };
            Bind<TaskNode[]>().ToConstant(tasks).Named(Name);
        }
    }
}
