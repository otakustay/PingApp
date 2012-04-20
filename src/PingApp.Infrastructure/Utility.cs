using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PingApp.Infrastructure {
    public static class Utility {
        private static readonly Regex idFromUrl = new Regex(@"\/id(\d+)", RegexOptions.Compiled);

        public static string Captalize(this string str, string separator = "-") {
            IEnumerable<string> parts = str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Char.ToUpper(s[0]) + s.Substring(1));
            return String.Join(String.Empty, parts);
        }

        public static IEnumerable<ICollection<T>> Partition<T>(this IEnumerable<T> list, int size) {
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

        public static int FindIdFromUrl(string url) {
            Match match = idFromUrl.Match(url);
            return (match != null && match.Groups.Count >= 2) ? Convert.ToInt32(match.Groups[1].Value) : -1;
        }
    }
}
