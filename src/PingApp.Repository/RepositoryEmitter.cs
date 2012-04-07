using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Repository {
    public sealed class RepositoryEmitter {
        public IAppRepository App { get; private set; }

        public IAppUpdateRepository AppUpdate { get; private set; }

        public IAppTrackRepository AppTrack { get; private set; }

        public IUserRepository User { get; private set; }

        public RepositoryEmitter(IAppRepository app, 
            IAppUpdateRepository appUpdate,
            IAppTrackRepository appTrack,
            IUserRepository user) {
                App = app;
                AppUpdate = appUpdate;
                AppTrack = appTrack;
                User = user;
        }
    }
}
