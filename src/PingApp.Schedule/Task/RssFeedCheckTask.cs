using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using NLog;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using PingApp.Schedule.Storage;

namespace PingApp.Schedule.Task {
    class RssFeedCheckTask : TaskNode {
        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start downloading rss feed");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // 他妹子的RSS只给100条，而且genre这个条件完全没用
            XDocument doc = XDocument.Load("http://itunes.apple.com/cn/rss/newapplications/limit=300/xml");
            int[] products = doc.Root.Descendants("{http://www.w3.org/2005/Atom}entry")
                .Select(d => d.Elements("{http://www.w3.org/2005/Atom}id").First().Value)
                .Select(s => Utility.ExtractIdFromUrl(s))
                .ToArray();
            watch.Stop();
            Log.Info("RSS feed retrieved {0} items using {1}ms", products.Length, watch.ElapsedMilliseconds);

            watch.Start();
            List<int> matched = new List<int>();

            try {
                using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["PingApp"].ConnectionString)) {
                    MySqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format("select Id from AppHash where Id in ({0})", String.Join(",", products));
                    connection.Open();
                    using (IDataReader reader = cmd.ExecuteReader()) {
                        cmd.CommandTimeout = 0;
                        while (reader.Read()) {
                            matched.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex) {
                Log.FatalException("Cannot retrieve match from db", ex);
                return null;
            }

            int[] diff = products.Except(matched).ToArray();
            Log.Info("Found {0} new products", diff.Length);
            watch.Stop();
            Log.Info("Work done using {0}ms", watch.ElapsedMilliseconds);

            IStorage output = new MemoryStorage();
            output.Add(diff);

            return output;
        }
    }
}
