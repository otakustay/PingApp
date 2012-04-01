using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    class AddAppModule : NinjectModule {
        public override string Name {
            get {
                return ActionType.AddApp.ToString();
            }
        }

        public override void Load() {
            Bind<SearchApiTask>().ToSelf().Named(Name)
                .WithConstructorArgument("computeDiff", false);
            Bind<DbUpdateTask>().ToSelf().Named(Name)
                .WithConstructorArgument("checkType", DbCheckType.ForceInsert)
                .WithConstructorArgument("checkOffUpdates", false);
            Bind<IndexTask>().ToSelf().Named(Name)
                .WithConstructorArgument("incremental", true);

            TaskNode[] tasks = new TaskNode[] {
                Kernel.Get<SearchApiTask>(Name),
                Kernel.Get<DbUpdateTask>(Name),
                Kernel.Get<IndexTask>(Name)
            };
            Bind<TaskNode[]>().ToConstant(tasks).Named(Name);
        }
    }
}
