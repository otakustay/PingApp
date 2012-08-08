using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.MySql {
    public class AppUpdateRepository : IAppUpdateRepository, IDisposable {
        private readonly MySqlConnection connection;

        public AppUpdateRepository(MySqlConnection connection) {
            this.connection = connection;
        }

        public AppUpdateQuery RetrieveByApp(AppUpdateQuery query) {
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "select * from AppUpdate where App = ?App and Time <= ?LatestTime";
            command.Parameters.AddWithValue("?App", query.App);
            command.Parameters.AddWithValue("?LatestTime", query.LatestTime);
            if (query.EarliestTime.HasValue) {
                command.CommandText += " and Time >= ?EarliestTime";
                command.Parameters.AddWithValue("?EarliestTime", query.EarliestTime);
            }
            command.CommandText += ";";

            List<AppUpdate> result = new List<AppUpdate>();
            using (IDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    AppUpdate update = reader.ToAppUpdate();
                    result.Add(update);
                }
            }

            query.Fill(result);
            return query;
        }

        public void Save(AppUpdate update) {
            update.Id = Guid.NewGuid();

            string sql =
@"insert into `AppUpdate` (
    `Id`, `App`, `Time`, `Type`, `OldValue`, `NewValue`
) values (
    ?Id, ?App, ?Time, ?Type, ?OldValue, ?NewValue
);";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?Id", update.Id.ToString("N"));
            command.Parameters.AddWithValue("?App", update.App);
            command.Parameters.AddWithValue("Time", update.Time);
            command.Parameters.AddWithValue("?Type", update.Type);
            command.Parameters.AddWithValue("?OldValue", update.OldValue);
            command.Parameters.AddWithValue("?NewValue", update.NewValue);
            command.ExecuteNonQuery();
        }

        public void Dispose() {
            connection.Dispose();
        }
    }
}
