using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository.Mongo {
    public sealed class AppRepository : IAppRepository {
        public App Retrieve(int id) {
            throw new NotImplementedException();
        }

        public void Save(App app) {
            throw new NotImplementedException();
        }

        public void Update(App app) {
            throw new NotImplementedException();
        }

        public ISet<int> FindExists(IEnumerable<int> apps) {
            throw new NotImplementedException();
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public ICollection<int> RetrieveIdentities(int offset, int limit) {
            throw new NotImplementedException();
        }

        public IDictionary<int, string> RetrieveHash(int offset, int limit) {
            throw new NotImplementedException();
        }

        public IDictionary<int, string> RetrieveHash(IEnumerable<int> apps) {
            throw new NotImplementedException();
        }
    }
}
