using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Ninject.Modules;

namespace PingApp.Repository.MySql.Dependency {
    public sealed class MySqlRepositoryModule : NinjectModule {
        public override void Load() {
            Bind<IAppRepository>().To<AppRepository>();
        }
    }
}
