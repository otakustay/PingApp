using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Infrastructure;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    sealed class RescueModule : NinjectModule {
        public override void Load() {
            Bind<RescueTask>().ToSelf()
                .WithConstructorArgument("indexer", ctx => ctx.Kernel.Get<LuceneIndexer>("Update"));
        }
    }
}
