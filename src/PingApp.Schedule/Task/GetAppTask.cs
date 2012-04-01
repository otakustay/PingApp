using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using PingApp.Schedule.Storage;
using Ninject;
using PingApp.Repository.NHibernate.Dependency;
using PingApp.Repository;
using System.Collections;

namespace PingApp.Schedule.Task {
    class GetAppTask : TaskNode {
        private readonly bool computeDiff;

        private readonly IKernel kernel;

        public GetAppTask(bool computeDiff, IKernel kernel) {
            this.computeDiff = computeDiff;
            this.kernel = kernel;
        }

        // TODO: Remove
        public GetAppTask(bool computeDiff) {
            this.computeDiff = computeDiff;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start retrieving all records from db using --compute-diff={0}", computeDiff);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            List<int> list = new List<int>(500000);
            int offset = 0;
            int size = Program.BatchSize * 8;

            using (SessionStore sessionStore = new SessionStore()) {
                kernel.Rebind<IDictionary>().ToConstant(sessionStore);
                RepositoryEmitter repository = kernel.Get<RepositoryEmitter>();

                ICollection<int> page;
                do {
                    Stopwatch regionWatch = new Stopwatch();
                    regionWatch.Start();

                    page = repository.App.RetrieveIdentities(offset, size);

                    regionWatch.Stop();
                    Log.Info("Retrieved {0} records from db using {1}ms", page.Count, regionWatch.ElapsedMilliseconds);

                    list.AddRange(page);
                    offset += size;
                }
                while (page.Count >= size);
            }
            watch.Stop();
            Log.Info("Retrieved a total of {0} records using {1}ms", list.Count, watch.ElapsedMilliseconds);

            IStorage output = new MemoryStorage();
            if (computeDiff) {
                ISet<int> set = input.Get<ISet<int>>();
                set.ExceptWith(list);
                Log.Info("Diff done, found {0} difference", set.Count);
                output.Add(set);
            }
            else {
                output.Add(list);
            }

            return output;
        }

        private List<int> GetPartition(MySqlConnection connection, int offset, int size) {
            List<int> list = new List<int>(size);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try {
                MySqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = String.Format("select Id from AppBrief limit {0}, {1}", offset, size);
                using (IDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        list.Add(reader.GetInt32(0));
                    }
                }

                watch.Stop();
                Log.Info("Retrieved {0} records from db using {1}ms", list.Count, watch.ElapsedMilliseconds);

                return list;
            }
            catch (Exception ex) {
                Log.FatalException("Error retrieving records from db", ex);
                return new List<int>();
            }
        }
    }
}
