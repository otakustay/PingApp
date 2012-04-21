using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using NLog;
using NLog.Config;
using NLog.Targets;
using PanGu.Match;
using PingApp.Infrastructure;
using LogLevel = NLog.LogLevel;

namespace PingApp.Schedule.Dependency {
    sealed class SharedModule : NinjectModule {
        private readonly ActionType action;

        public SharedModule(ActionType action) {
            this.action = action;
        }

        public override void Load() {
            // 用于共享的对象的依赖管理
        }
    }
}
