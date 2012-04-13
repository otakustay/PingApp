using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using PingApp.Repository.Quries;

namespace PingApp.Repository {
    public interface IAppTrackRepository {
        void Save(AppTrack track);

        void Update(AppTrack track);

        void Remove(Guid id);

        AppTrack Retrieve(Guid user, int app);

        AppTrackQuery Retrieve(AppTrackQuery query);

        // 以下为Schedule使用
        ICollection<AppTrack> RetrieveByApp(int app);

        int ResetReadStatusByApp(int app);
    }
}
