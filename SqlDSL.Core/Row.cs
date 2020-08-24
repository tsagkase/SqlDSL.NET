using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SqlDSL.Core
{
    public class Row : IRow
    {
        private readonly Hashtable _entries;

        public Row(Hashtable entries)
        {
            this._entries = entries;
        }

        private T SafelyAccess<T>(string key)
        {
            if (!_entries.ContainsKey(key))
                throw new Exception(string.Format("unknown column '{0}'", key));
            return (T)_entries[key];
        }

        #region IRow Members
        public object this[string idx]
        {
            get
            {
                return SafelyAccess<object>(idx);
            }
            set
            {
                _entries[idx] = value;
            }
        }

        public Dictionary<string, object> GetAllEntries()
        {
            return _entries.Keys.Cast<object>().ToDictionary(entry => (string) entry, entry => _entries[entry]);
        }

        public static Func<IRow, string> GetAsString(string key)
        {
            return row => (string)row[key];
        }

        public static Func<IRow, T?> GetAs<T>(string key) where T : struct 
        {
            return row => (T?)row[key];
        }

        public string String(string key)
        {
            return SafelyAccess<string>(key);
        }

        public decimal? Decimal(string key)
        {
            return SafelyAccess<decimal?>(key);
        }

        public bool? Bool(string key)
        {
            return SafelyAccess<bool?>(key);
        }

        public byte? Byte(string key)
        {
            return SafelyAccess<byte?>(key);
        }

        public DateTime? DateTime(string key)
        {
            return SafelyAccess<DateTime?>(key);
        }

        public Guid? Guid(string key)
        {
            return SafelyAccess<Guid?>(key);
        }

        public short? Short(string key)
        {
            return SafelyAccess<short?>(key);
        }

        public int? Int(string key)
        {
            return SafelyAccess<int?>(key);
        }

        public double? Double(string key)
        {
            return SafelyAccess<double?>(key);
        }

        public byte[] Binary(string key)
        {
            return SafelyAccess<byte[]>(key);
        }

        public Int64? Int64(string key)
        {
            return SafelyAccess<Int64?>(key);
        }

        public long? Long(string key)
        {
            return SafelyAccess<long?>(key);
        }

        //public Natural Natural(string key)
        //{
        //    return (Natural)entries[key];
        //}

        //public Integer Integer(string key)
        //{
        //    return (Integer)entries[key];
        //}

        //public Rational Rational(string key)
        //{
        //    return (Rational)entries[key];
        //}

        #endregion
    }
}