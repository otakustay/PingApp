using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Schedule {
    interface IStorage {
        void Add<T>(T value);

        void Add<T>(string name, T value);

        bool HasMore { get; }

        T Get<T>();

        T Get<T>(string name);
    }
}
