using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IAppRepository {
        App Retrieve(int app);

        ICollection<App> Retrieve(IEnumerable<int> required);

        // 以下为Schedule使用
        void Save(App app);

        void Update(App app);

        ICollection<App> Retrieve(int offset, int limit);

        RevokedApp Revoke(App app);
    }
}
