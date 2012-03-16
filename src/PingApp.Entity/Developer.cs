using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class Developer {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ViewUrl { get; set; }

        public Developer() {
            Name = String.Empty;
            ViewUrl = String.Empty;
        }

        public Developer(int id, string name, string viewUrl) {
            Id = id;
            Name = name;
            ViewUrl = viewUrl;
        }

        public override bool Equals(object obj) {
            Developer other = obj as Developer;
            if (other == null) {
                return false;
            }

            return new EqualsBuilder()
                .Append(Id, other.Id)
                .Append(Name, other.Name)
                .Append(ViewUrl, other.ViewUrl)
                .AreEqual;
        }

        public override int GetHashCode() {
            return Id;
        }
    }
}
