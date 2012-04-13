using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository {
    public interface IAppUpdateRepository {
        AppUpdateQuery RetrieveByApp(AppUpdateQuery query);

        // 以下为Schedule使用
        void Save(AppUpdate update);
    }
}
