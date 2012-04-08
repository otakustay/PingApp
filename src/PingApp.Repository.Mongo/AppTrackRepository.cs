using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class AppTrackRepository : IAppTrackRepository {
        private readonly MongoCollection<AppTrack> appTracks;

        public AppTrackRepository(MongoCollection<AppTrack> appTracks) {
            this.appTracks = appTracks;
        }

        public ICollection<AppTrack> RetrieveByApp(int app) {
            ICollection<AppTrack> result = appTracks.Find(Query.EQ("app", app)).ToArray();
            return result;
        }

        public int ResetReadStatusByApp(int app) {
            SafeModeResult result = appTracks.Update(Query.EQ("app", app), Update.Set("hasRead", false), SafeMode.True);
            return (int)result.DocumentsAffected;
        }
    }
}
