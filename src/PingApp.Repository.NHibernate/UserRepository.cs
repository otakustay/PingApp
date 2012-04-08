using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate {
    public class UserRepository : IUserRepository {
        public User Retrieve(Guid id) {
            throw new NotImplementedException();
        }
    }
}
