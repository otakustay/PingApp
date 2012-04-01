using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;
using PingApp.Utility;
using PingApp.Entity;
using PingApp.Schedule.Storage;
using System.IO;
using System.Diagnostics;
using Ninject;
using PingApp.Repository.NHibernate.Dependency;
using PingApp.Repository;
using System.Collections;

namespace PingApp.Schedule.Task {
    class GetFullAppTask : TaskNode {
        private readonly IKernel kernel;

        public GetFullAppTask(IKernel kernel) {
            this.kernel = kernel;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start retrieve all apps from database");

            ICollection<int> list = input.Get<ICollection<int>>();
            IStorage output = new FileSystemStorage(Path.Combine(LogRoot, "Output"));

            using (SessionStore sessionStore = new SessionStore()) {
                kernel.Rebind<IDictionary>().ToConstant(sessionStore);
                RepositoryEmitter repository = kernel.Get<RepositoryEmitter>();

                foreach (IEnumerable<int> part in Utility.Partition(list, Program.BatchSize)) {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    ICollection<App> apps = repository.App.Retrieve(part);

                    watch.Stop();
                    Log.Info("{0} apps retrieved using {1}ms", apps.Count, watch.ElapsedMilliseconds);

                    output.Add(apps);
                }
            }

            return output;
        }
    }
}
