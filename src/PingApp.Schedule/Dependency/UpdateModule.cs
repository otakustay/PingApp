using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Infrastructure;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    sealed class UpdateModule : NinjectModule {
        public override void Load() {
            Bind<UpdateTask>().ToSelf()
                .WithConstructorArgument("indexer", ctx => ctx.Kernel.Get<LuceneIndexer>("Update"));
        }
    }
}
