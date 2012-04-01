using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IAppRepository {
        AppBrief RetrieveBrief(int id);

        // 以下为Schedule使用
        void Save(App app);

        void Save(AppBrief brief);

        void Update(App app);

        void Update(AppBrief brief);

        ISet<int> FindExists(IEnumerable<int> apps);

        ICollection<App> Retrieve(IEnumerable<int> required);

        IDictionary<int, string> RetrieveHash(int offset, int limit);

        IDictionary<int, string> RetrieveHash(IEnumerable<int> apps);
    }
}
