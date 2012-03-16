using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using NHibernate.UserTypes;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;

namespace PingApp.Web.Models.Mapping {
    class CategoryArrayType : IUserType {
        public object Assemble(object cached, object owner) {
            return cached;
        }

        public object DeepCopy(object value) {
            Category[] categories = (Category[])value;
            return categories.Select(c => new Category() { Id = c.Id, Name = c.Name }).ToArray();
        }

        public object Disassemble(object value) {
            return value;
        }

        public new bool Equals(object x, object y) {
            int[] left = ((Category[])x).Select(c => c.Id).ToArray();
            int[] right = ((Category[])y).Select(c => c.Id).ToArray();

            if (left.Length != right.Length) {
                return false;
            }
            HashSet<int> set = new HashSet<int>(left);
            return right.All(i => set.Contains(i));
        }

        public int GetHashCode(object x) {
            return x.GetHashCode();
        }

        public bool IsMutable {
            get { return false; }
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner) {
            string value = (string)NHibernateUtil.String.NullSafeGet(rs, names[0]);
            return value.Split(',').Where(s => s.Length > 0).Select(s => Category.Get(Convert.ToInt32(s))).ToArray();
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index) {
            string s = String.Join(",", ((Category[])value).Select(c => c.Id).ToArray());
            NHibernateUtil.Int32.NullSafeSet(cmd, s, index);
        }

        public object Replace(object original, object target, object owner) {
            return original;
        }

        public Type ReturnedType {
            get { return typeof(Category[]); }
        }

        public SqlType[] SqlTypes {
            get { return new SqlType[] { new SqlType(DbType.String) }; }
        }
    }
}
