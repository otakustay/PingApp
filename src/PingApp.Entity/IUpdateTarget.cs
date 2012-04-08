using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public interface IUpdateTarget {
        void UpdateFrom(object obj);
    }
}
