using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using PingApp.Schedule.Storage;

namespace PingApp.Schedule.Task {
    class GetAppHashTask : TaskNode {
        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start retrieving all records and hash from db using");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Dictionary<int, string> list = new Dictionary<int, string>(500000);
            int offset = 0;
            int size = Program.BatchSize * 8;
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                connection.Open();
                if (input == null) {
                    Dictionary<int, string> page;
                    do {
                        page = GetPartition(connection, offset, size);
                        foreach (KeyValuePair<int, string> item in page) {
                            list.Add(item.Key, item.Value);
                        }
                        offset += size;
                    }
                    while (page.Count >= size);
                }
                else {
                    MySqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format(
                        "select Id, Hash from AppBrief where Id in ({0})", 
                        String.Join(",", input.Get<IEnumerable<int>>())
                    );
                    using (IDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            list[reader.GetInt32(0)] = reader.GetString(1);
                        }
                    }
                }
            }
            watch.Stop();
            Log.Info("Retrieved a total of {0} records using {1}ms", list.Count, watch.ElapsedMilliseconds);

            IStorage output = new MemoryStorage();
            output.Add(list);

            output.Add(list);
            return output;
        }

        private Dictionary<int, string> GetPartition(MySqlConnection connection, int offset, int size) {
            Dictionary<int, string> list = new Dictionary<int, string>(size);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try {
                MySqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = String.Format("select Id, Hash from AppBrief limit {0}, {1}", offset, size);
                using (IDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        list[reader.GetInt32(0)] = reader.GetString(1);
                    }
                }

                watch.Stop();
                Log.Info("Retrieved {0} records from db using {1}ms", list.Count, watch.ElapsedMilliseconds);

                return list;
            }
            catch (Exception ex) {
                Log.FatalException("Error retrieving records from db", ex);
                return new Dictionary<int, string>();
            }
        }
    }
}
