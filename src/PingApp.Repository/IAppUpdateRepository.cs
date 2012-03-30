using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IAppUpdateRepository {

        // 以下为Schedule使用
        void Save(AppUpdate update);
    }
}
