using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Repository.MySql {
    public static class Map {
        public static User ToUser(this IDataRecord record) {
            throw new NotImplementedException();
        }

        public static AppBrief ToAppBrief(this IDataRecord record) {
            throw new NotImplementedException();
        }

        public static App ToApp(this IDataRecord record) {
            throw new NotImplementedException();
        }

        public static App ToRevokedApp(this IDataRecord record) {
            throw new NotImplementedException();
        }

        public static AppUpdate ToAppUpdate(this IDataRecord record) {
            throw new NotImplementedException();
        }

        public static AppTrack ToAppTrack(this IDataRecord record) {
            throw new NotImplementedException();
        }
    }
}
