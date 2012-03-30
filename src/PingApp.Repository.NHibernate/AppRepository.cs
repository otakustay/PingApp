using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingApp.Repository.NHibernate {
    public class AppRepository : IAppRepository {
        public ISet<int> FindExists(IEnumerable<int> apps) {
            throw new NotImplementedException();
        }

        public ICollection<Entity.App> Retrieve(IEnumerable<int> required) {
            throw new NotImplementedException();
        }

        public void Save(Entity.App app) {
            throw new NotImplementedException();
        }

        public void Update(Entity.App app) {
            throw new NotImplementedException();
        }
    }
}
