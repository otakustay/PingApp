using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    [Flags]
    public enum UserStatus {
        Ok = 0,

        AppImported = 1,

        PasswordReset = 2
    }
}
