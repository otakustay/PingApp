using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Infrastructure;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    sealed class RssCheckModule: NinjectModule {
        public override void Load() {
            Bind<RssCheckTask>().ToSelf()
                .WithConstructorArgument("indexer", ctx => ctx.Kernel.Get<IAppIndexer>("Update"));
        }
    }
}
