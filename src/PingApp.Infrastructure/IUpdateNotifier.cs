using System;
using PingApp.Entity;

namespace PingApp.Infrastructure {
    public interface IUpdateNotifier : IDisposable {
        void ProcessUpdate(App app, AppUpdate update);
    }
}
