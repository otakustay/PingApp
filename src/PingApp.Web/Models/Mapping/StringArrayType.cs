using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.UserTypes;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;

namespace PingApp.Web.Models.Mapping {

    class StringArrayType : IUserType {
        public object Assemble(object cached, object owner) {
            return cached;
        }

        public object DeepCopy(object value) {
            return ((string[])value).Clone();
        }

        public object Disassemble(object value) {
            return value;
        }

        public new bool Equals(object x, object y) {
            string[] left = (string[])x;
            string[] right = (string[])y;

            if (left.Length != right.Length) {
                return false;
            }
            HashSet<string> set = new HashSet<string>(left);
            return right.All(s => set.Contains(s));
        }

        public int GetHashCode(object x) {
            return x.GetHashCode();
        }

        public bool IsMutable {
            get { return false; }
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner) {
            string value = (string)NHibernateUtil.String.NullSafeGet(rs, names[0]);
            return value.Split(',').Where(s => s.Length > 0).ToArray();
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index) {
            NHibernateUtil.String.NullSafeSet(cmd, String.Join(",", (string[])value), index);
        }

        public object Replace(object original, object target, object owner) {
            return original;
        }

        public Type ReturnedType {
            get { return typeof(string[]); }
        }

        public SqlType[] SqlTypes {
            get { return new SqlType[] { new SqlType(DbType.String) }; }
        }
    }
}
