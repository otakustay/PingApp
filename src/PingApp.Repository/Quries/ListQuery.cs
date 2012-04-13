using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Repository.Quries {
    public abstract class ListQuery<T> {
        public ICollection<T> Result { get; private set; }

        public virtual void Fill(ICollection<T> result) {
            Result = result.ToArray();
        }
    }
}
