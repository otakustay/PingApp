using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    class UpdateAppModule : NinjectModule {
        public override string Name {
            get {
                return ActionType.UpdateApp.ToString();
            }
        }

        public override void Load() {
            Bind<GetAppHashTask>().ToSelf().Named(Name);
            Bind<SearchApiTask>().ToSelf().Named(Name)
                .WithConstructorArgument("computeDiff", true);
            Bind<DbUpdateTask>().ToSelf().Named(Name)
                .WithConstructorArgument("checkType", DbCheckType.CheckForUpdate)
                .WithConstructorArgument("checkOffUpdates", true);
            Bind<IndexTask>().ToSelf().Named(Name)
                .WithConstructorArgument("incremental", true);

            TaskNode[] tasks = new TaskNode[] {
                Kernel.Get<GetAppHashTask>(Name),
                Kernel.Get<SearchApiTask>(Name),
                Kernel.Get<DbUpdateTask>(Name),
                Kernel.Get<IndexTask>(Name)
            };
            Bind<TaskNode[]>().ToConstant(tasks).Named(Name);
        }
    }
}
