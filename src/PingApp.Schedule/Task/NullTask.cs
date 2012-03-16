using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Schedule.Task {
    class NullTask : TaskNode {
        protected override IStorage RunTask(IStorage input) {
            return null;
        }
    }
}
