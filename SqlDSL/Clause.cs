using System;
using System.Data;
using System.Data.SqlTypes;

namespace SqlDSL
{
    [Obsolete("Use InParam class instead")]
    public class Clause
    {
        private static CriterionNVT Criterion(string paramName, DbType paramType, object paramValue)
        {
            return new CriterionNVT(paramName, paramType, paramValue);
        }

        [Obsolete("Use InParam.Guid(..)")]
        public static CriterionNVT GuidEquals(string paramName, string paramValue)
        {
            return new CriterionNVT(paramName, DbType.Guid, new Guid(paramValue));
        }

        [Obsolete("Use InParam.Guid(..)")]
        public static CriterionNVT GuidEquals(string paramName, Guid paramValue)
        {
            return new CriterionNVT(paramName, DbType.Guid, paramValue);
        }

        public static CriterionNVT Guid(string paramName, string paramValue)
        {
            return new CriterionNVT(paramName, DbType.Guid, new Guid(paramValue));
        }

        public static CriterionNVT Guid(string paramName, Guid paramValue)
        {
            return new CriterionNVT(paramName, DbType.Guid, paramValue);
        }

        [Obsolete("Use InParam.String(..)")]
        public static CriterionNVT StringEquals(string paramName, string paramValue)
        {
            return new CriterionNVT(paramName, DbType.String, paramValue);
        }

        public static CriterionNVT String(string paramName, string paramValue)
        {
            return new CriterionNVT(paramName, DbType.String, paramValue);
        }

        [Obsolete("Use InParam.Bool(..)")]
        public static CriterionNVT BoolEquals(string paramName, bool paramValue)
        {
            return new CriterionNVT(paramName, DbType.Boolean, paramValue);
        }

        public static CriterionNVT Bool(string paramName, bool paramValue)
        {
            return new CriterionNVT(paramName, DbType.Boolean, paramValue);
        }

        [Obsolete("Use InParam.Int(..)")]
        public static CriterionNVT IntEquals(string paramName, int paramValue)
        {
            return new CriterionNVT(paramName, DbType.Int32, paramValue);
        }

        public static CriterionNVT Int(string paramName, int paramValue)
        {
            return new CriterionNVT(paramName, DbType.Int32, paramValue);
        }

        [Obsolete("Use InParam.Double(..)")]
        public static CriterionNVT DoubleEquals(string paramName, double paramValue)
        {
            return new CriterionNVT(paramName, DbType.Double, paramValue);
        }

        public static CriterionNVT Double(string paramName, double paramValue)
        {
            return new CriterionNVT(paramName, DbType.Double, paramValue);
        }

        [Obsolete("Use InParam.DateTime(..)")]
        public static CriterionNVT DateTimeEquals(string paramName, DateTime paramValue)
        {
            return DateTime(paramName, paramValue, false);
        }

        public static CriterionNVT DateTime(string paramName, DateTime paramValue)
        {
            return DateTime(paramName, paramValue, false);
        }

        public static CriterionNVT DateTime(string paramName, DateTime paramValue, bool insertDBNullInCaseOfDateTimeMinValue)
        {
            //TSIU:18/11/07
            //BUG FIX
            //DateTime.MinValue=00:00:00.0000000, January 1, 0001.
            //SqlDateTime.MinValue=January 1, 1753.
            if (paramValue.Equals(System.DateTime.MinValue))
            {
                if (insertDBNullInCaseOfDateTimeMinValue)
                {
                    return new CriterionNVT(paramName, DbType.DateTime, DBNull.Value);
                }
                return new CriterionNVT(paramName, DbType.DateTime, SqlDateTime.MinValue.Value);
            }
            return new CriterionNVT(paramName, DbType.DateTime, paramValue);
        }

        [Obsolete("Use InParam.Decimal(..)")]
        public static CriterionNVT DecimalEquals(string paramName, decimal paramValue)
        {
            return new CriterionNVT(paramName, DbType.Decimal, paramValue);
        }

        public static CriterionNVT Decimal(string paramName, decimal paramValue)
        {
            return new CriterionNVT(paramName, DbType.Decimal, paramValue);
        }

        [Obsolete("Use InParam.Binary(..)")]
        public static CriterionNVT BinaryEquals(string paramName, byte[] paramValue)
        {
            return new CriterionNVT(paramName, DbType.Binary, paramValue);
        }

        public static CriterionNVT Binary(string paramName, byte[] paramValue)
        {
            return new CriterionNVT(paramName, DbType.Binary, paramValue);
        }

        [Obsolete("Use InParam.Long(..)")]
        public static CriterionNVT Int64Equals(string paramName, Int64 paramValue)
        {
            return new CriterionNVT(paramName, DbType.Int64, paramValue);
        }

        public static CriterionNVT Long(string paramName, long paramValue)
        {
            return new CriterionNVT(paramName, DbType.Int64, paramValue);
        }

        [Obsolete("Use InParam.Xml(..)")]
        public static CriterionNVT XmlEquals(string paramName, string xmlValue)
        {
            return new CriterionNVT(paramName, DbType.Xml, xmlValue);
        }

        public static CriterionNVT Xml(string paramName, string xmlValue)
        {
            return new CriterionNVT(paramName, DbType.Xml, xmlValue);
        }

        //public static CriterionNVT NaturalEquals(string paramName, Natural paramValue)
        //{
        //    return new CriterionNVT(paramName, DbType.String, paramValue.ToString());
        //}
    }
}