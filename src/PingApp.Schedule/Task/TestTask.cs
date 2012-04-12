using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using NLog;
using PingApp.Schedule.Infrastructure;

namespace PingApp.Schedule.Task {
    class TestTask : TaskBase {
        private readonly IKernel kernel;

        public TestTask(IKernel kernel, ProgramSettings settings, Logger logger)
            : base(settings, logger) {
            this.kernel = kernel;
        }

        public override void Run(string[] args) {
            Console.WriteLine("Test Complete");
        }
    }
}
