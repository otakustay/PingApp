using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace PingApp.Schedule.Task {
    class TestTask : TaskNode {
        private readonly IKernel kernel;

        public TestTask(IKernel kernel) {
            this.kernel = kernel;
        }

        protected override IStorage RunTask(IStorage input) {
            Console.WriteLine("Test Complete");
            return null;
        }
    }
}
