using System;
using System.Collections.Generic;

namespace SqlDSL
{
    public interface IRow
    {
        object this[string idx] { get; set; }

        string String(string key);

        decimal? Decimal(string key);

        bool? Bool(string key);

        byte? Byte(string key);

        DateTime? DateTime(string key);

        Guid? Guid(string key);

        short? Short(string key);

        int? Int(string key);

        double? Double(string key);

        byte[] Binary(string key);

        Int64? Int64(string key);

        long? Long(string key);

        



        //Natural Natural(string key);

        //Integer Integer(string key);

        //Rational Rational(string key);
        Dictionary<string, object> GetAllEntries(IRow row);
    }
}