using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace PingApp.Utility {
    public static class DataUtility {
        public static T Get<T>(this IDataReader reader, string name) {
            object value = reader[name];
            return (value == null || value is DBNull) ? default(T) : (T)value;
        }

        #region 各数据类型

        public static int GetInt32(this IDataReader reader, string name) {
            int ordinal = reader.GetOrdinal(name);
            return reader.GetInt32(ordinal);
        }

        public static int? GetNullableInt32(this IDataReader reader, string name) {
            return Get<int?>(reader, name);
        }

        public static float GetSingle(this IDataReader reader, string name) {
            return Get<float>(reader, name);
        }

        public static float? GetNullableSingle(this IDataReader reader, string name) {
            return Get<float?>(reader, name);
        }

        public static long GetInt64(this IDataReader reader, string name) {
            int ordinal = reader.GetOrdinal(name);
            return reader.GetInt64(ordinal);
        }

        public static bool GetBoolean(this IDataReader reader, string name) {
            int ordinal = reader.GetOrdinal(name);
            return reader.GetBoolean(ordinal);
        }

        public static string GetString(this IDataReader reader, string name) {
            int ordinal = reader.GetOrdinal(name);
            return reader.GetString(ordinal);
        }

        public static DateTime GetDateTime(this IDataReader reader, string name) {
            int ordinal = reader.GetOrdinal(name);
            return reader.GetDateTime(ordinal);
        }

        public static string[] GetStringArray(this IDataReader reader, string name) {
            int ordinal = reader.GetOrdinal(name);
            string s = reader.GetString(ordinal);
            return s.Split(',');
        }

        #endregion
    }
}
