using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Schedule {
    class Buffer<T> {
        private readonly int size;

        private readonly Action<T[]> flush;

        private List<T> list;

        private readonly object syncRoot = new object();

        public Buffer(int size, Action<T[]> flush) {
            this.size = size;
            this.flush = flush;
            list = new List<T>(size * 2);
        }

        public void Add(T value) {
            List<T> buffer = null;
            lock (syncRoot) {
                list.Add(value);
                if (list.Count >= size) {
                    buffer = list;
                    list = new List<T>(size * 2);
                }
            }
            if (buffer != null) {
                flush(buffer.ToArray());
            }
        }

        public void AddRange(IEnumerable<T> value) {
            List<T> buffer = null;
            lock (syncRoot) {
                list.AddRange(value);
                if (list.Count >= size) {
                    buffer = list;
                    list = new List<T>(size * 2);
                }
            }
            if (buffer != null) {
                flush(buffer.ToArray());
            }
        }

        public void Flush() {
            List<T> buffer = null;
            lock (syncRoot) {
                if (list.Count > 0) {
                    buffer = list;
                    // 通常Flush之后就没进一步的Add了，所以不用建太大的List
                    list = new List<T>();
                }
            }
            if (buffer != null) {
                flush(buffer.ToArray());
            }
        }
    }
}
