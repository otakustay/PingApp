using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Schedule.Storage {
    class MemoryStorage : IStorage {
        private readonly Queue<object> list = new Queue<object>();

        private readonly Dictionary<string, object> named = new Dictionary<string, object>();

        private readonly object syncRoot = new object();

        public void Add<T>(T value) {
            lock (syncRoot) {
                list.Enqueue(value);
            }
        }

        public void Add<T>(string name, T value) {
            named[name] = value;
        }

        public bool HasMore {
            get {
                lock (syncRoot) {
                    return list.Count > 0;
                }
            }
        }

        public T Get<T>() {
            lock (syncRoot) {
                return (T)list.Dequeue();
            }
        }

        public T Get<T>(string name) {
            return named.ContainsKey(name) ? (T)named[name] : default(T);
        }
    }
}
