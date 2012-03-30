using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Schedule.Task {
    class TestTask : TaskNode {
        protected override IStorage RunTask(IStorage input) {
            Console.WriteLine("Test Complete");
            return null;
        }
    }
}
