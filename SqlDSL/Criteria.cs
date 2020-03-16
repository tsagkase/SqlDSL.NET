using System.Data;

namespace SqlDSL
{
    public class CriterionNVT : NameValueType<string, DbType, object>
    {
        public CriterionNVT(string name, DbType type, object value, string typeName = "") 
            : base(name, type, value, typeName) { }
    }

    public class NameValueType<N, T, V>
    {
        private readonly N _name;
        private readonly V _value;
        private readonly string _typeName;
        private readonly T _type;

        public NameValueType(N name, T type, V value, string typeName = "")
        {
            this._name = name;
            this._type = type;
            this._value = value;
            _typeName = typeName;
        }

        public N Name { get { return _name; } }
        public T Type { get { return _type; } }
        public V Value { get { return _value; } }
        public string TypeName { get { return _typeName; } }
    }
}