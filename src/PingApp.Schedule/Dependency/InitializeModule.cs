using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using PingApp.Schedule.Task;

namespace PingApp.Schedule.Dependency {
    class InitializeModule : NinjectModule {
        public override void Load() {
            Bind<InitializeTask>().ToSelf();
        }
    }
}
