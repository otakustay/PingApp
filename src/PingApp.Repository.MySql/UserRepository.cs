using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using PingApp.Entity;

namespace PingApp.Repository.MySql {
    public class UserRepository : IUserRepository, IDisposable {
        private readonly MySqlConnection connection;

        public UserRepository(MySqlConnection connection) {
            this.connection = connection;
        }

        public void Save(User user) {
            user.Id = Guid.NewGuid();

            string sql =
@"insert into `User` (
    `Id`, `Email`, `Username`, `Password`, `Description`, `Website`,
    `NotifyOnWishPriceDrop`, `NotifyOnWishFree`, `NotifyOnWishUpdate`,
    `NotifyOnOwnedUpdate`, `ReceiveSiteUpdates`, `PreferredLanguagePriority`,
    `Status`, `RegisterTime`
) values (
    ?Id, ?Email, ?Username, ?Password, ?Description, ?Website,
    ?NotifyOnWishPriceDrop, ?NotifyOnWishFree, ?NotifyOnWishUpdate,
    ?NotifyOnOwnedUpdate, ?ReceiveSiteUpdates, ?PreferredLanguagePriority,
    ?Status, ?RegisterTime
);";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?Id", user.Id.ToString("N"));
            command.Parameters.AddWithValue("?Email", user.Email);
            command.Parameters.AddWithValue("?Username", user.Username);
            command.Parameters.AddWithValue("?Password", user.Password);
            command.Parameters.AddWithValue("?Description", user.Description);
            command.Parameters.AddWithValue("?Website", user.Website);
            command.Parameters.AddWithValue("?NotifyOnWishPriceDrop", user.NotifyOnWishPriceDrop);
            command.Parameters.AddWithValue("?NotifyOnWishFree", user.NotifyOnWishFree);
            command.Parameters.AddWithValue("?NotifyOnWishUpdate", user.NotifyOnWishUpdate);
            command.Parameters.AddWithValue("?NotifyOnOwnedUpdate", user.NotifyOnOwnedUpdate);
            command.Parameters.AddWithValue("?ReceiveSiteUpdates", user.ReceiveSiteUpdates);
            command.Parameters.AddWithValue("?PreferredLanguagePriority", user.PreferredLanguagePriority);
            command.Parameters.AddWithValue("?Status", user.Status);
            command.Parameters.AddWithValue("?RegisterTime", user.RegisterTime);
            command.ExecuteNonQuery();
        }

        public void Update(User user) {
            throw new NotImplementedException();
        }

        public User Retrieve(Guid id) {
            throw new NotImplementedException();
        }

        public User RetrieveByEmail(string email) {
            throw new NotImplementedException();
        }

        public User RetrieveByUsername(string username) {
            string sql = "select * from `User` where `Username` = ?Username";
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("?Username", username);
            using (IDataReader reader = command.ExecuteReader()) {
                if (reader.Read()) {
                    User user = reader.ToUser();
                    return user;
                }
                else {
                    return null;
                }
            }
        }

        public bool Exists(string email, string username) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            connection.Dispose();
        }
    }
}
