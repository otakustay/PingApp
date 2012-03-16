using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;

namespace PingApp.Schedule {
    class FullApp : App {
        public string Hash { get; set; }

        public int Changeset {
            get {
                return Convert.ToInt32(Hash.Substring(0, 2), 16);
            }
        }

        public string ActualHash {
            get {
                return Hash.Substring(2);
            }
        }
    }
}
