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

namespace PingApp.Infrastructure.Dependency {
    public sealed class InfrastructureModule : NinjectModule {
        public override void Load() {
            Bind<ProgramSettings>().ToConstant(ProgramSettings.Current).InSingletonScope();
            Bind<SmtpClient>().ToSelf();

            Bind<JsonSerializerSettings>().ToSelf()
                .WithPropertyValue("ContractResolver", new CamelCasePropertyNamesContractResolver())
                .WithPropertyValue("DateTimeZoneHandling", DateTimeZoneHandling.Utc);
            // Ninject无法注入Field，只能手动生成
            MatchOptions matchOptions = new MatchOptions();
            matchOptions.ChineseNameIdentify = true;
            matchOptions.EnglishMultiDimensionality = true;
            matchOptions.TraditionalChineseEnabled = true;
            Bind<MatchOptions>().ToConstant(matchOptions).InSingletonScope();

            // Infrastructure接口
            Bind<IWebDownload>().To<StandardWebDownload>().InSingletonScope();

            Bind<ICatalogParser>().To<StandardCatalogParser>();

            Bind<IAppParser>().To<StandardAppParser>()
                .WithConstructorArgument("truncateLimit", 200);

            Bind<IAppIndexer>().To<LuceneIndexer>().Named("Rebuild")
                .WithConstructorArgument("rebuild", true);
            Bind<IAppIndexer>().To<LuceneIndexer>().Named("Update")
                .WithConstructorArgument("rebuild", false);

            Bind<IUpdateNotifier>().To<StandardUpdateNotifier>();
        }
    }
}
