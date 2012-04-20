using System;
using PingApp.Entity;

namespace PingApp.Infrastructure {
    public interface IAppIndexer : IDisposable {
        void AddApp(App app);

        void DeleteApp(App app);

        void Flush();

        void UpdateApp(App app);
    }
}
