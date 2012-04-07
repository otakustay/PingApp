using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IUserRepository {
        User Retrieve(int id);
    }
}
