using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using PanGu;
using System.IO;

namespace PingApp.Utility.Lucene {
    class PanGuTokenizer : Tokenizer {
        private static object syncRoot = new object();
        private static bool initialized = false;

        private WordInfo[] words;

        private int position = -1; //词汇在缓冲中的位置.

        private string inputText;

        static private void InitPanGuSegment() {
            //Init PanGu Segment.
            if (!initialized) {
                Segment.Init(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "PanGu.xml"));
                initialized = true;
            }
        }

        /// <summary>
        /// Init PanGu Segment
        /// </summary>
        /// <param name="fileName">PanGu.xml file path</param>
        static public void InitPanGuSegment(string fileName) {
            lock (syncRoot) {
                //Init PanGu Segment.
                if (!initialized) {
                    global::PanGu.Segment.Init(fileName);
                    initialized = true;
                }
            }
        }

        public PanGuTokenizer() {
            lock (syncRoot) {
                InitPanGuSegment();
            }
        }

        public PanGuTokenizer(TextReader input)
            : base(input) {
            lock (syncRoot) {
                InitPanGuSegment();
            }

            inputText = base.input.ReadToEnd();

            if (string.IsNullOrEmpty(inputText)) {
                char[] readBuf = new char[1024];

                int relCount = base.input.Read(readBuf, 0, readBuf.Length);

                StringBuilder inputStr = new StringBuilder(readBuf.Length);

                while (relCount > 0) {
                    inputStr.Append(readBuf, 0, relCount);

                    relCount = input.Read(readBuf, 0, readBuf.Length);
                }

                if (inputStr.Length > 0) {
                    inputText = inputStr.ToString();
                }
            }

            if (string.IsNullOrEmpty(inputText)) {
                words = new WordInfo[0];
            }
            else {
                global::PanGu.Segment segment = new Segment();
                ICollection<WordInfo> wordInfos = segment.DoSegment(inputText);
                words = new WordInfo[wordInfos.Count];
                wordInfos.CopyTo(words, 0);
            }
        }

        //DotLucene的分词器简单来说，就是实现Tokenizer的Next方法，把分解出来的每一个词构造为一个Token，因为Token是DotLucene分词的基本单位。
        [Obsolete]
        public override Token Next() {
            int length = 0;
            int start = 0;

            while (true) {
                position++;
                if (position < words.Length) {
                    if (words[position] != null) {
                        length = words[position].Word.Length;
                        start = words[position].Position;
                        return new Token(words[position].Word, start, start + length);
                    }
                }
                else {
                    break;
                }
            }

            inputText = null;
            return null;
        }

        public ICollection<WordInfo> SegmentToWordInfos(String str) {
            if (string.IsNullOrEmpty(str)) {
                return new LinkedList<WordInfo>();
            }

            Segment segment = new Segment();
            return segment.DoSegment(str);
        }
    }
}
