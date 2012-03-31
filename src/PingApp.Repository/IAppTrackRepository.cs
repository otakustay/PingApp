using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository {
    public interface IAppTrackRepository {

        // 以下为Schedule使用
        ICollection<AppTrack> RetrieveByApp(int app);

        int ResetForApp(int app);
    }
}
