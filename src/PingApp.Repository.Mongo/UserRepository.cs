using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class UserRepository : IUserRepository {
        public User Retrieve(Guid id) {
            throw new NotImplementedException();
        }
    }
}
