using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IAppRepository {

        // 以下为Schedule使用
        ISet<int> FindExists(IEnumerable<int> apps);

        ICollection<App> Retrieve(IEnumerable<int> required);

        void Save(App app);

        void Update(App app);
    }
}
