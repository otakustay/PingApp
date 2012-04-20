using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Infrastructure;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    class CheckNewModule : NinjectModule {
        public override void Load() {
            Bind<CheckNewTask>().ToSelf()
                .WithConstructorArgument("indexer", ctx => ctx.Kernel.Get<IAppIndexer>("Update"));
        }
    }
}
