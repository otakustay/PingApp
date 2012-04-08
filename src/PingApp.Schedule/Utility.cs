using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Schedule {
    static class Utility {
        public static string Captalize(this string str, string separator = "-") {
            IEnumerable<string> parts = str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Char.ToUpper(s[0]) + s.Substring(1));
            return String.Join(String.Empty, parts);
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> list, int size) {
            List<T> output = new List<T>(size);
            foreach (T item in list) {
                output.Add(item);
                if (output.Count % size == 0) {
                    yield return output;
                    output = new List<T>(size);
                }
            }
            if (output.Count > 0) {
                yield return output;
            }
        }
    }
}
