using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using PingApp.Schedule.Storage;
using Ninject;
using PingApp.Repository.NHibernate.Dependency;
using System.Collections;
using PingApp.Repository;

namespace PingApp.Schedule.Task {
    class GetAppHashTask : TaskNode {
        private readonly IKernel kernel;

        public GetAppHashTask(IKernel kernel) {
            this.kernel = kernel;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start retrieving all records and hash from db using");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            IDictionary<int, string> list;
            int offset = 0;
            int size = Program.BatchSize * 8;

            using (SessionStore sessionStore = new SessionStore()) {
                kernel.Rebind<IDictionary>().ToConstant(sessionStore);
                RepositoryEmitter repository = kernel.Get<RepositoryEmitter>();

                if (input == null) {
                    list = new Dictionary<int, string>(500000);
                    IDictionary<int, string> page;
                    do {
                        Stopwatch regionWatch = new Stopwatch();
                        regionWatch.Start();

                        page = repository.App.RetrieveHash(offset, size);

                        regionWatch.Stop();
                        Log.Info("Retrieved {0} records from db using {1}ms", page.Count, regionWatch.ElapsedMilliseconds);

                        foreach (KeyValuePair<int, string> item in page) {
                            list.Add(item.Key, item.Value);
                        }

                        offset += size;
                    }
                    while (page.Count >= size);
                }
                else {
                    IEnumerable<int> required = input.Get<IEnumerable<int>>();
                    list = repository.App.RetrieveHash(required);
                }
            }

            watch.Stop();
            Log.Info("Retrieved a total of {0} records using {1}ms", list.Count, watch.ElapsedMilliseconds);

            IStorage output = new MemoryStorage();
            output.Add(list);

            output.Add(list);
            return output;
        }
    }
}
