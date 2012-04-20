using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace PingApp.Infrastructure {
    sealed class StandardWebDownload : IWebDownload {
        private static readonly Encoding encoding = Encoding.UTF8;

        private readonly WebProxy proxy;

        public StandardWebDownload(ProgramSettings settings) {
            if (!String.IsNullOrEmpty(settings.ProxyAddress)) {
                proxy = new WebProxy(settings.ProxyAddress);
            }
        }

        public string AsString(string uri) {
            using (WebClient client = new WebClient()) {
                client.Encoding = encoding;
                if (proxy != null) {
                    client.Proxy = proxy;
                }

                return client.DownloadString(uri);
            }
        }

        public HtmlDocument AsDocument(string uri) {
            string str = AsString(uri);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(str);
            return document;
        }

        public JObject AsJson(string uri) {
            string str = AsString(uri);
            return JObject.Parse(str);
        }

        public XDocument AsXml(string uri) {
            XDocument document = XDocument.Load(uri);
            return document;
        }
    }
}
