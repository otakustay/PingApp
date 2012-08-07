using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool Exists(string email, string username) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            connection.Dispose();
        }
    }
}
