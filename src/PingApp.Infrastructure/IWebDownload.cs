using System;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace PingApp.Infrastructure {
    public interface IWebDownload {
        string AsString(string uri);

        HtmlDocument AsDocument(string uri);

        JObject AsJson(string uri);

        XDocument AsXml(string uri);
    }
}
