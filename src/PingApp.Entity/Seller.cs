using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class Seller {
        public string Name { get; set; }

        public string ViewUrl { get; set; }

        public Seller() {
            Name = String.Empty;
            ViewUrl = String.Empty;
        }

        public Seller(string name, string viewUrl) {
            Name = name;
            ViewUrl = viewUrl;
        }

        public override bool Equals(object obj) {
            Seller other = obj as Seller;
            if (other == null) {
                return false;
            }

            return new EqualsBuilder()
                .Append(Name, other.Name)
                .Append(ViewUrl, other.ViewUrl)
                .AreEqual;
        }

        public override int GetHashCode() {
            return Name.GetHashCode() * 31 + ViewUrl.GetHashCode();
        }
    }
}
