using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using PingApp.Schedule.Infrastructure;

namespace PingApp.Schedule.Dependency {
    sealed class SharedModule : NinjectModule {
        private readonly ActionType action;

        public SharedModule(ActionType action) {
            this.action = action;
        }

        public override void Load() {
            Bind<ProgramSettings>().ToConstant(ProgramSettings.Current).InSingletonScope();
            Bind<Logger>().ToMethod(GetLogger).InSingletonScope();

            Bind<WebDownload>().ToSelf().InSingletonScope();

            Bind<JsonSerializerSettings>().ToSelf()
                .WithPropertyValue("ContractResolver", new CamelCasePropertyNamesContractResolver())
                .WithPropertyValue("DateTimeZoneHandling", DateTimeZoneHandling.Utc);
            // Ninject无法注入Field，只能手动生成
            MatchOptions matchOptions = new MatchOptions();
            matchOptions.ChineseNameIdentify = true;
            matchOptions.EnglishMultiDimensionality = true;
            matchOptions.TraditionalChineseEnabled = true;
            Bind<MatchOptions>().ToConstant(matchOptions).InSingletonScope();

            Bind<CatalogParser>().ToSelf();
            Bind<AppParser>().ToSelf()
                .WithConstructorArgument("truncateLimit", 200);
            Bind<LuceneIndexer>().ToSelf().Named("Rebuild")
                .WithConstructorArgument("rebuild", true);
            Bind<LuceneIndexer>().ToSelf().Named("Update")
                .WithConstructorArgument("rebuild", false);
        }

        private Logger GetLogger(IContext context) {
            string logRoot = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase, 
                "Log", 
                DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + this.action
            );
            ProgramSettings settings = context.Kernel.Get<ProgramSettings>();
            string layout = "${time}|${level}|${message}${onexception:inner=${newline}}${exception:format=tostring}";

            LoggingConfiguration config = new LoggingConfiguration();

            ColoredConsoleTarget console = new ColoredConsoleTarget();
            config.AddTarget("console", console);
            FileTarget file = new FileTarget();
            config.AddTarget("file", file);
            FileTarget debug = new FileTarget();
            config.AddTarget("debug", debug);
            FileTarget trace = new FileTarget();
            config.AddTarget("trace", trace);
            FileTarget error = new FileTarget();
            config.AddTarget("error", error);

            console.Layout = settings.Debug ? "${time}|${level}|${message}" : layout;
            file.FileName = logRoot + "/log.txt";
            file.Encoding = Encoding.UTF8;
            file.Layout = layout;
            debug.FileName = logRoot + "/debug.txt";
            debug.Encoding = Encoding.UTF8;
            debug.Layout = layout;
            trace.FileName = logRoot + "/verbose.txt";
            trace.Encoding = Encoding.UTF8;
            trace.Layout = layout;
            error.FileName = logRoot + "/error.txt";
            error.Encoding = Encoding.UTF8;
            error.Layout = layout;

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, console));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, file));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debug));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, trace));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, error));

            LogManager.Configuration = config;

            return LogManager.GetLogger(this.action.ToString());
        }
    }
}
