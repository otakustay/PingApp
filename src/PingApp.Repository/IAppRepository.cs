using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IAppRepository {
        App Retrieve(int app);

        // 以下为Schedule使用
        void Save(App app);

        void Update(App app);

        ISet<int> FindExists(IEnumerable<int> apps);

        ICollection<App> Retrieve(IEnumerable<int> required);

        ICollection<int> RetrieveIdentities(int offset, int limit);

        IDictionary<int, string> RetrieveHash(int offset, int limit);

        IDictionary<int, string> RetrieveHash(IEnumerable<int> apps);
    }
}
