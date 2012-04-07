using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class AppTrackRepository : IAppTrackRepository {
        public ICollection<AppTrack> RetrieveByApp(int app) {
            throw new NotImplementedException();
        }

        public int ResetReadStatusByApp(int app) {
            throw new NotImplementedException();
        }
    }
}
