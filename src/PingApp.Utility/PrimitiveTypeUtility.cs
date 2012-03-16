using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PanGu;
using PanGu.Match;

namespace PingApp.Utility {
    public static class PrimitiveTypeUtility {

        public static string SplitWordTo(this string input, int length) {
            MatchOptions options = new MatchOptions();
            options.ChineseNameIdentify = true;
            options.EnglishMultiDimensionality = true;
            options.TraditionalChineseEnabled = true;
            Segment segment = new Segment();
            ICollection<WordInfo> words = segment.DoSegment(input, options);
            int stop = 0;
            foreach (WordInfo word in words) {
                if (word.Position + word.Word.Length > length) {
                    return stop < length / 2 ? input.Substring(0, length) : input.Substring(0, stop).Trim();
                }
                stop = word.Position + word.Word.Length;
            }
            return input;
        }

        public static string JoinField<T, TResult>(this T[] array, string separator, Func<T, TResult> selector) {
            return String.Join(separator, array.Select(selector).ToArray());
        }
    }
}
