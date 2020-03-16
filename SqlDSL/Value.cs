using System;
using System.Data;

namespace SqlDSL
{
    public class Value
    {
        private class DataRowExtractor<T> : IDataRowExtractor
        {
            public delegate T Processor(IDataReader row, string selection, T inCaseOfDbNull);

            private readonly string _selection;
            private readonly Processor _extract;
            private readonly T _inCaseOfDbNull;

            public DataRowExtractor(string selection, Processor extract, T inCaseOfDbNull = default(T))
            {
                this._selection = selection;
                this._extract = extract;
                this._inCaseOfDbNull = inCaseOfDbNull;
            }
            public void Extract(IDataReader row, System.Collections.IDictionary result)
            {
                result.Add(_selection, _extract(row, _selection, _inCaseOfDbNull));
            }
            public string Selection { get { return _selection; } }
        }

        private class NullableDataRowExtractor<T> : IDataRowExtractor where T : struct
        {
            public delegate T? Processor(IDataReader row, string selection);

            private readonly string _selection;
            private readonly Processor _extract;

            public NullableDataRowExtractor(string selection, Processor extract)
            {
                this._selection = selection;
                this._extract = extract;
            }
            public void Extract(IDataReader row, System.Collections.IDictionary result)
            {
                result.Add(_selection, _extract(row, _selection));
            }
            public string Selection { get { return _selection; } }
        }

        /*
        public static IDataRowExtractor Guid(string selection)
        {
            return Guid(selection, System.Guid.Empty);
        }

        public static IDataRowExtractor Guid(string selection, Guid inCaseOfDbNull)
        {
            return new DataRowExtractor<Guid>(selection, extractRowGuid, inCaseOfDbNull);
        }
         */

        public static IDataRowExtractor Guid(string selection)
        {
            return new NullableDataRowExtractor<Guid>(selection, extractRow(value => new Guid(value.ToString())));
        }

        public static IDataRowExtractor String(string selection)
        {
            return String(selection, string.Empty); // TODO: This is currently ignored
        }

        public static IDataRowExtractor String(string selection, string inCaseOfDbNull)
        {
            // TODO:  the problem here is that string is already Nullable<T> where T is not exposed to anybody!!!!!! (the joys of lang design)
            //return new NullableDataRowExtractor<string>(selection, extractRow(Convert.ToString));

            inCaseOfDbNull = null;
            return new DataRowExtractor<string>(selection, extractRowString, inCaseOfDbNull);
        }

        public static IDataRowExtractor Decimal(string selection)
        {
            return Decimal(selection, 0);
        }

        public static IDataRowExtractor Decimal(string selection, decimal inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<decimal>(selection, extractRow(Convert.ToDecimal));
        }

        public static IDataRowExtractor Bool(string selection)
        {
            return Bool(selection, false);
        }

        public static IDataRowExtractor Bool(string selection, bool inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<bool>(selection, extractRow(Convert.ToBoolean));
        }

        public static IDataRowExtractor Byte(string selection)
        {
            return Byte(selection, 0);
        }

        public static IDataRowExtractor Byte(string selection, byte inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<byte>(selection, extractRow(Convert.ToByte));
        }

        public static IDataRowExtractor DateTime(string selection)
        {
            return DateTime(selection, System.DateTime.MinValue);
        }

        public static IDataRowExtractor DateTime(string selection, DateTime inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<DateTime>(selection, extractRow(Convert.ToDateTime));
        }

        public static IDataRowExtractor Short(string selection)
        {
            return Short(selection, 0);
        }

        public static IDataRowExtractor Short(string selection, short inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<short>(selection, extractRow(Convert.ToInt16));
        }

        public static IDataRowExtractor Int(string selection)
        {
            return Int(selection, 0);
        }

        public static IDataRowExtractor Int(string selection, int inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<int>(selection, extractRow(Convert.ToInt32));
        }

        public static IDataRowExtractor Double(string selection)
        {
            return Double(selection, 0.0);
        }

        public static IDataRowExtractor Double(string selection, double inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<double>(selection, extractRow(Convert.ToDouble));
        }

        public static IDataRowExtractor Binary(string selection)
        {
            return Binary(selection, new byte[] { });
        }

        public static IDataRowExtractor Binary(string selection, byte[] inCaseOfDbNull)
        {
            return new DataRowExtractor<byte[]>(selection, extractRowBinary, inCaseOfDbNull);
        }
        public static IDataRowExtractor Int64(string selection)
        {
            return Int64(selection, 0);
        }

        public static IDataRowExtractor Int64(string selection, Int64 inCaseOfDbNull)
        {
            return new NullableDataRowExtractor<Int64>(selection, extractRow(Convert.ToInt64));
        }

        //public static IDataRowExtractor Natural(string selection)
        //{
        //    return Natural(selection, 0);
        //}

        //public static IDataRowExtractor Natural(string selection, Natural inCaseOfDbNull)
        //{
        //    return new DataRowExtractor<Natural>(selection, extractRowNatural, inCaseOfDbNull);
        //}

        //public static IDataRowExtractor Integer(string selection)
        //{
        //    return Integer(selection, 0);
        //}

        //public static IDataRowExtractor Integer(string selection, Integer inCaseOfDbNull)
        //{
        //    return new DataRowExtractor<Integer>(selection, extractRowInteger, inCaseOfDbNull);
        //}

        //public static IDataRowExtractor Rational(string selection)
        //{
        //    return Rational(selection, 0);
        //}

        //public static IDataRowExtractor Rational(string selection, Rational inCaseOfDbNull)
        //{
        //    return new DataRowExtractor<Rational>(selection, extractRowRational, inCaseOfDbNull);
        //}

        private static byte[] extractRowBinary(IDataReader row, string columnName, byte[] inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, value =>(byte[])value);
        }

        private static string extractRowString(IDataReader row, string columnName, string inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, value => value.ToString());
        }

        private static decimal? extractRowDecimalNullable(IDataReader row, string columnName)
        {
            return extractRow(row, columnName, Convert.ToDecimal);
        }

        private static decimal extractRowDecimal(IDataReader row, string columnName, decimal inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToDecimal);
        }

        private static bool extractRowBool(IDataReader row, string columnName, bool inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToBoolean);
        }

        private static byte extractRowByte(IDataReader row, string columnName, byte inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToByte);
        }

        private static DateTime extractRowDateTime(IDataReader row, string columnName, DateTime inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToDateTime);
        }

        private static Guid? extractRowGuidNullable(IDataReader row, string columnName)
        {
            return extractRow(row, columnName, value => new Guid(value.ToString()));
        }

        private static Guid extractRowGuid(IDataReader row, string columnName, Guid inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, value => new Guid(value.ToString()));
        }

        private static short extractRowShort(IDataReader row, string columnName, short inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToInt16);
        }

        private static int extractRowInt(IDataReader row, string columnName, int inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToInt32);
        }

        private static double extractRowDouble(IDataReader row, string columnName, double inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToDouble);
        }

        private static Int64 extractRowInt64(IDataReader row, string columnName, Int64 inCaseOfDbNull)
        {
            return extractRow(row, columnName, inCaseOfDbNull, Convert.ToInt64);
        }

        private static T extractRow<T>(IDataReader row
                                       , string columnName
                                       , T inCaseOfDbNull
                                       , Func<object, T> converter)
        {
            object value = row[columnName];
            return value == DBNull.Value ? inCaseOfDbNull : converter(value);
        }

        private static NullableDataRowExtractor<T>.Processor extractRow<T>(Func<object, T> converter)
            where T : struct
        {
            return (row, columnName) =>
                       {
                           object value = row[columnName];
                           return value == DBNull.Value ? (T?) null : converter(value);
                       };
        }

        private static T? extractRow<T>(IDataReader row
                                       , string columnName
                                       , Func<object, T> converter)
            where T : struct
        {
            object value = row[columnName];
            return value == DBNull.Value ? (T?) null : converter(value);
        }

        //private static Natural extractRowNatural(IDataReader row, string columnName, Natural inCaseOfDbNull)
        //{
        //    int index = row.GetOrdinal(columnName);
        //    object value = ((SqlDataReader)row).GetProviderSpecificValue(index);
        //    return value == DBNull.Value ? inCaseOfDbNull : new Natural(value.ToString());
        //}
        //private static Integer extractRowInteger(IDataReader row, string columnName, Integer inCaseOfDbNull)
        //{
        //    int index = row.GetOrdinal(columnName);
        //    object value = ((SqlDataReader)row).GetProviderSpecificValue(index);
        //    return value == DBNull.Value ? inCaseOfDbNull : new Integer(value.ToString());
        //}
        //private static Rational extractRowRational(IDataReader row, string columnName, Rational inCaseOfDbNull)
        //{
        //    int index = row.GetOrdinal(columnName);
        //    object value = ((SqlDataReader)row).GetProviderSpecificValue(index);
        //    return value == DBNull.Value ? inCaseOfDbNull : new Rational(value.ToString());
        //}
    }
}