using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using NLog;
using PingApp.Infrastructure;

namespace PingApp.Schedule.Task {
    class TestTask : TaskBase {
        private static readonly ILogger logger = ProgramSettings.GetLogger<TestTask>();

        private readonly IKernel kernel;

        public TestTask(IKernel kernel, ProgramSettings settings)
            : base(settings) {
            this.kernel = kernel;
        }

        public override void Run(string[] args) {
            Console.WriteLine("Test Complete");
        }
    }
}
