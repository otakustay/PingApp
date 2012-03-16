using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using System.IO;

namespace PingApp.Utility.Lucene {
    public class PanGuAnalyzer : Analyzer {
        public PanGuAnalyzer() {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader) {
            TokenStream result = new PanGuTokenizer(reader);
            result = new LowerCaseFilter(result);
            return result;
        }
    }
}
