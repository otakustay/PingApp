using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using PingApp.Entity;

namespace PingApp.Repository.Mongo.Dependency {
    public sealed class MongoRepositoryModule : NinjectModule {
        public override void Load() {
            ConventionProfile profile = new ConventionProfile();
            profile.SetElementNameConvention(new CamelCaseElementNameConvention());
            profile.SetIdMemberConvention(new NamedIdMemberConvention("Id"));
            BsonClassMap.RegisterConventions(profile, t => true);
            BsonClassMap.RegisterClassMap<App>(MapApp);
            BsonClassMap.RegisterClassMap<AppTrack>(MapAppTrack);
            // 作为AppBrief的LastValidUpdate时没有App字段，因此忽略
            BsonSerializer.RegisterSerializer(typeof(Category), new CategorySerializer());
            BsonSerializer.RegisterIdGenerator(typeof(AppUpdate), CombGuidGenerator.Instance);
            BsonSerializer.RegisterIdGenerator(typeof(AppTrack), CombGuidGenerator.Instance);
            BsonSerializer.RegisterIdGenerator(typeof(User), CombGuidGenerator.Instance);

            string connectionString = ConfigurationManager.ConnectionStrings["Mongo"].ConnectionString;
            string databaseName = ConfigurationManager.AppSettings["MongoDatabase"];
            Bind<MongoServer>().ToMethod(c => MongoServer.Create(connectionString));
            Bind<MongoDatabase>().ToMethod(c => c.Kernel.Get<MongoServer>().GetDatabase(databaseName));

            Bind<MongoCollection<App>>().ToMethod(c => GetCollection<App>(c, "apps"));
            Bind<MongoCollection<RevokedApp>>().ToMethod(c => GetCollection<RevokedApp>(c, "revokedApps"));
            Bind<MongoCollection<AppTrack>>().ToMethod(c => GetCollection<AppTrack>(c, "appTracks"));
            Bind<MongoCollection<AppUpdate>>().ToMethod(c => GetCollection<AppUpdate>(c, "appUpdates"));
            Bind<MongoCollection<User>>().ToMethod(c => GetCollection<User>(c, "users"));

            Bind<IAppRepository>().To<AppRepository>();
            Bind<IAppUpdateRepository>().To<AppUpdateRepository>();
            Bind<IAppTrackRepository>().To<AppTrackRepository>();
            Bind<IUserRepository>().To<UserRepository>();
        }

        private void MapApp(BsonClassMap<App> map) {
            map.AutoMap();
            map.MapProperty(t => t.Brief.DeviceType);
            map.MapProperty(t => t.Brief.IsGameCenterEnabled);
            map.MapProperty(t => t.Brief.CalculatedWeights);
        }

        private void MapAppTrack(BsonClassMap<AppTrack> map) {
            map.AutoMap();
            map.MapProperty(t => t.App).SetSerializer(new AppBriefSerializer());
        }

        private MongoCollection<T> GetCollection<T>(IContext context, string collectionName) {
            return context.Kernel.Get<MongoDatabase>().GetCollection<T>(collectionName);
        }
    }
}
