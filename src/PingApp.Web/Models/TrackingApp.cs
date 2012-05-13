using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingApp.Entity;

namespace PingApp.Web.Models {
    /// <summary>
    /// 带用户关注信息的App
    /// </summary>
    public class TrackingApp {
        public AppBrief App { get; private set; }

        public AppTrack Track { get; private set; }

        public TrackingApp(AppBrief app, AppTrack track) {
            App = app;
            Track = track;
        }
    }
}