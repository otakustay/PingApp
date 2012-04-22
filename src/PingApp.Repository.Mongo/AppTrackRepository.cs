using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Builders = MongoDB.Driver.Builders;
using PingApp.Entity;
using PingApp.Repository.Quries;
using MongoDB.Bson;

namespace PingApp.Repository.Mongo {
    public sealed class AppTrackRepository : IAppTrackRepository {
        private readonly MongoCollection<App> apps;

        private readonly MongoCollection<AppTrack> appTracks;

        public AppTrackRepository(MongoCollection<App> apps, MongoCollection<AppTrack> appTracks) {
            this.apps = apps;
            this.appTracks = appTracks;
        }

        public void Save(AppTrack track) {
            appTracks.Save(track);
        }

        public void Update(AppTrack track) {
            appTracks.Save(track);
        }

        public void Remove(Guid id) {
            appTracks.Remove(Query.EQ("_id", id), RemoveFlags.Single);
        }

        public AppTrack Retrieve(Guid user, int app) {
            IMongoQuery mongoQuery = Query.And(
                Query.EQ("user", user),
                Query.EQ("app._id", app)
            );

            AppTrack result = appTracks.FindOne(mongoQuery);
            return result;
        }

        public AppTrackQuery Retrieve(AppTrackQuery query) {
            List<IMongoQuery> mongoQueries = new List<IMongoQuery>();

            mongoQueries.Add(Query.EQ("user", query.User));

            if (query.Status.HasValue) {
                mongoQueries.Add(Query.EQ("status", query.Status.Value));
            }

            if (query.RelatedApps != null) {
                mongoQueries.Add(Query.In("app", BsonArray.Create(query.RelatedApps)));
            }

            AppTrack[] result = appTracks.Find(Query.And(mongoQueries.ToArray()))
                .SetSkip(query.SkipSize)
                .SetLimit(query.TakeSize)
                .ToArray();

            // 填上App的全部信息
            BsonArray idx = BsonArray.Create(result.Select(t => t.App.Id));
            Dictionary<int, AppBrief> relatedApps = apps.Find(Query.In("_id", idx))
                .SetFields("brief")
                .ToDictionary(a => a.Id, a => a.Brief);
            foreach (AppTrack track in result) {
                if (relatedApps.ContainsKey(track.App.Id)) {
                    track.App = relatedApps[track.App.Id];
                }
            }

            query.Fill(result);

            return query;
        }

        public ICollection<AppTrack> RetrieveByApp(int app) {
            ICollection<AppTrack> result = appTracks.Find(Query.EQ("app", app)).ToArray();
            return result;
        }

        public int ResetReadStatusByApp(int app) {
            SafeModeResult result = appTracks.Update(Query.EQ("app", app), Builders.Update.Set("hasRead", false), SafeMode.True);
            return (int)result.DocumentsAffected;
        }
    }
}
