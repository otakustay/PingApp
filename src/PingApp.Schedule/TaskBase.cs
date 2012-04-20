using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.IO;
using Ninject;
using PingApp.Infrastructure;

namespace PingApp.Schedule {
    abstract class TaskBase : IDisposable {
        protected readonly ProgramSettings settings;

        protected readonly Logger logger;

        protected TaskBase(ProgramSettings settings, Logger logger) {
            this.settings = settings;
            this.logger = logger;
        }

        public abstract void Run(string[] args);

        protected virtual string Name {
            get {
                return GetType().Name;
            }
        }

        public virtual void Dispose() {
        }
    }
}
