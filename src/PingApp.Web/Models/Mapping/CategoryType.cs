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
    class CategoryType : IUserType {
        public object Assemble(object cached, object owner) {
            return cached;
        }

        public object DeepCopy(object value) {
            Category category = (Category)value;
            return new Category() {
                Id = category.Id,
                Name = category.Name
            };
        }

        public object Disassemble(object value) {
            return value;
        }

        public new bool Equals(object x, object y) {
            return x.Equals(y);
        }

        public int GetHashCode(object x) {
            return x.GetHashCode();
        }

        public bool IsMutable {
            get { return false; }
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner) {
            int id = (int)NHibernateUtil.Int32.NullSafeGet(rs, names[0]);
            return Category.Get(id);
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index) {
            NHibernateUtil.Int32.NullSafeSet(cmd, ((Category)value).Id, index);
        }

        public object Replace(object original, object target, object owner) {
            return original;
        }

        public Type ReturnedType {
            get { return typeof(Category); }
        }

        public SqlType[] SqlTypes {
            get { return new SqlType[] { new SqlType(DbType.Int32) }; }
        }
    }
}
