using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PingApp.Schedule.Storage {
    class FileSystemStorage : IStorage {
        private readonly string directory;

        private readonly object syncRoot = new object();

        private int counter = 0;

        private int cursor = 0;

        public FileSystemStorage(string directory) {
            this.directory = directory;
            Directory.CreateDirectory(directory);
        }


        public void Add<T>(T value) {
            string filename;
            lock (syncRoot) {
                counter++;
                filename = Path.Combine(directory, counter + ".txt");
            }
            WriteToFile(filename, value);
        }

        public void Add<T>(string name, T value) {
            string filename = Path.Combine(directory, name + ".txt");
            WriteToFile(filename, value);
        }

        public bool HasMore {
            get {
                lock (syncRoot) {
                    return cursor < counter;
                }
            }
        }

        public T Get<T>() {
            string filename;
            lock (syncRoot) {
                cursor++;
                filename = Path.Combine(directory, cursor + ".txt");
            }
            return ReadFromFile<T>(filename);
        }

        public T Get<T>(string name) {
            string filename = Path.Combine(directory, name + ".txt");
            return ReadFromFile<T>(filename);
        }

        private static void WriteToFile(string filename, object value) {
            string text = Utility.JsonSerialize(value);
            File.WriteAllText(filename, text, Encoding.UTF8);
        }

        private static T ReadFromFile<T>(string filename) {
            if (!File.Exists(filename)) {
                return default(T);
            }
            string text = File.ReadAllText(filename, Encoding.UTF8);
            return Utility.JsonDeserialize<T>(text);
        }
    }
}
