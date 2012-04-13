using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IUserRepository {
        void Save(User user);

        void Update(User user);

        User Retrieve(Guid id);

        User RetrieveByEmail(string email);

        User RetrieveByUsername(string username);

        bool Exists(string email, string username);
    }
}
