using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    class EqualsBuilder {
        bool areEqual = true;

        public EqualsBuilder Append<T>(T left, T right) {
            if (areEqual) {
                areEqual = left.Equals(right);
            }

            return this;
        }

        public EqualsBuilder AppendSequence<T>(T[] left, T[] right) {
            if (areEqual) {
                if (left.Length != right.Length) {
                    areEqual = false;
                    return this;
                }

                HashSet<T> set = new HashSet<T>(left);
                areEqual = right.All(t => set.Contains(t));
            }

            return this;
        }

        public bool AreEqual {
            get {
                return areEqual;
            }
        }
    }
}
