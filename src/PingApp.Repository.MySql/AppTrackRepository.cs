using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository.MySql {
    public class AppTrackRepository : IAppTrackRepository, IDisposable {
        private readonly MySqlConnection connection;

        public AppTrackRepository(MySqlConnection connection) {
            this.connection = connection;
        }

        public void Save(AppTrack track) {
            track.Id = Guid.NewGuid();

            string sql =
@"insert into `AppTrack` (
    `Id`, `User`, `App`, `Status`, `CreateTime`, `CreatePrice`,
    `BuyTime`, `BuyPrice`, `Rate`, `HasRead`
) values (
    ?Id, ?User, ?App, ?Status, ?CreateTime, ?CreatePrice,
    ?BuyTime, ?BuyPrice, ?Rate, ?HasRead
);";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            AddParametersForAppTrack(track, command);
            command.ExecuteNonQuery();
        }

        public void Update(AppTrack track) {
            string sql =
@"update `AppTrack`
set
    `Id` = ?Id,
    `User` = ?User,
    `App` = ?App,
    `Status` = ?Status,
    `CreateTime` = ?CreateTime,
    `CreatePrice` = ?CreatePrice,
    `BuyTime` = ?BuyTime,
    `BuyPrice` = ?BuyPrice,
    `Rate` = ?Rate,
    `HasRead` = ?HasRead
where `Id` = ?Id;";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            AddParametersForAppTrack(track, command);
            command.ExecuteNonQuery();
        }

        public void Remove(Guid id) {
            string sql = "delete from `AppTrack` where `Id` = ?Id";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?Id", id.ToString("N"));
            command.ExecuteNonQuery();
        }

        public AppTrack Retrieve(Guid user, int app) {
            string sql = "delete from `AppTrack` where `User` = ?User and `App` = ?App";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?User", user.ToString("N"));
            command.Parameters.AddWithValue("?App", app);
            using (IDataReader reader = command.ExecuteReader()) {
                if (reader.Read()) {
                    AppTrack track = reader.ToAppTrack();
                    return track;
                }
                else {
                    return null;
                }
            }
        }

        public AppTrackQuery Retrieve(AppTrackQuery query) {
            throw new NotImplementedException();
        }

        public ICollection<AppTrack> RetrieveByApp(int app) {
            string sql = "delete from `AppTrack` where `App` = ?App";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?App", app);
            List<AppTrack> result = new List<AppTrack>();
            using (IDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    AppTrack track = reader.ToAppTrack();
                    result.Add(track);
                }
            }
            return result;
        }

        public int ResetReadStatusByApp(int app) {
            string sql = "update `AppTrack` set `HasRead` = 0 where `App` = ?App";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?App", app);
            int count = command.ExecuteNonQuery();
            return count;
        }

        public void Dispose() {
            connection.Dispose();
        }

        private static void AddParametersForAppTrack(AppTrack track, MySqlCommand command) {
            command.Parameters.AddWithValue("?Id", track.Id.ToString("N"));
            command.Parameters.AddWithValue("?User", track.User.ToString("N"));
            command.Parameters.AddWithValue("?App", track.App.Id);
            command.Parameters.AddWithValue("?Status", track.Status);
            command.Parameters.AddWithValue("?CreateTime", track.CreateTime);
            command.Parameters.AddWithValue("?CreatePrice", track.CreatePrice);
            command.Parameters.AddWithValue("?BuyTime", track.BuyTime);
            command.Parameters.AddWithValue("?BuyPrice", track.BuyPrice);
            command.Parameters.AddWithValue("?Rate", track.Rate);
            command.Parameters.AddWithValue("?HasRead", track.HasRead);
        }
    }
}
