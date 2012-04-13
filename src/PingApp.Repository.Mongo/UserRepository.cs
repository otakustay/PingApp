using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class UserRepository : IUserRepository {
        private readonly MongoCollection<User> users;

        public UserRepository(MongoCollection<User> users) {
            this.users = users;
        }

        public void Save(User user) {
            users.Save(user);
        }

        public void Update(User user) {
            users.Save(user);
        }

        public User Retrieve(Guid id) {
            User result = users.FindOne(Query.EQ("_id", id));
            return result;
        }

        public User RetrieveByEmail(string email) {
            User result = users.FindOne(Query.EQ("email", email));
            return result;
        }

        public User RetrieveByUsername(string username) {
            User result = users.FindOne(Query.EQ("username", username));
            return result;
        }

        public bool Exists(string email, string username) {
            IMongoQuery mongoQuery = Query.Or(
                Query.EQ("email", email),
                Query.EQ("username", username)
            );
            long count = users.Find(mongoQuery).Count();
            return count > 0;
        }
    }
}
