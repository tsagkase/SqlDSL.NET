using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

namespace SqlDSL
{
    public class InParam
    {
        public static CriterionNVT Guid(string paramName, string paramValue)
        {
            return new CriterionNVT(paramName, DbType.Guid, new Guid(paramValue));
        }

        public static CriterionNVT Guid(string paramName, Guid paramValue)
        {
            return new CriterionNVT(paramName, DbType.Guid, paramValue);
        }
        public static CriterionNVT Email(string paramName, string paramValue)
        {
            return String(paramName, paramValue.Trim());
        }

        public static CriterionNVT String(string paramName, string paramValue,int? columnSize=null)
        {
            var value = paramValue;
            if (columnSize.HasValue && columnSize.Value>0)
                value = paramValue.Substring(0, columnSize.Value > paramValue.Length ? paramValue.Length : columnSize.Value);
            return new CriterionNVT(paramName, DbType.String, value);
        }

        public static CriterionNVT Bool(string paramName, bool paramValue)
        {
            return new CriterionNVT(paramName, DbType.Boolean, paramValue);
        }

        public static CriterionNVT Int(string paramName, int paramValue)
        {
            return new CriterionNVT(paramName, DbType.Int32, paramValue);
        }

        public static CriterionNVT Double(string paramName, double paramValue)
        {
            return new CriterionNVT(paramName, DbType.Double, paramValue);
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

        public static CriterionNVT Decimal(string paramName, decimal paramValue)
        {
            return new CriterionNVT(paramName, DbType.Decimal, paramValue);
        }

        public static CriterionNVT Binary(string paramName, byte[] paramValue)
        {
            return new CriterionNVT(paramName, DbType.Binary, paramValue);
        }

        public static CriterionNVT Long(string paramName, long paramValue)
        {
            return new CriterionNVT(paramName, DbType.Int64, paramValue);
        }

        public static CriterionNVT Xml(string paramName, string xmlValue)
        {
            return new CriterionNVT(paramName, DbType.Xml, xmlValue);
        }

        /// <summary>
        /// <para>Permits the passing of a list of DTO values as a parameter. See documentation of parameters for more details.</para>
        /// 
        /// <para>The DTO type with getter properties of the values we need persisted.</para>
        ///  
        /// <para>Note that the name of the DTO type will be mapped to the user defined SQL type of <code>udtt_{DTO name}</code> where <code>{DTO name}</code> is the DTO name.
        /// It is the responsibility of the programmer of course to have already deployed such a type to the SQL Server database!</para>
        /// 
        /// <para>Also, more importantly note that the order of getter properties's definition needs to map exactly to the order of definitions of <code>udt_{DTO name}</code> <see cref="http://social.msdn.microsoft.com/Forums/en/transactsql/thread/103abb3a-2f02-45c4-a4ba-392652c71f70"/></para>
        /// 
        /// <para>Applicable only to ADO backend and only for M$ Sql Server 2008 (or greater?)
        /// Also, if you're having doubts about performance see <see cref="http://www.sqlskills.com/BLOGS/BOBB/post/TVPs-and-plan-compilation-the-reprise.aspx"/>.</para>
        /// </summary>
        /// <typeparam name="DTO">A DTO type with getter properties of the values we need persisted. 
        /// Note that the name of the DTO type will be mapped to the user defined SQL type of <code>udtt_{DTO name}</code> where <code>{DTO name}</code> is the DTO name.
        /// It is the responsibility of the programmer of course to have already deployed such a type to the SQL Server database!
        /// Also, more importantly note that the order of getter properties's definition needs to map exactly to the order of definitions of <code>udt_{DTO name}</code> <see cref="http://social.msdn.microsoft.com/Forums/en/transactsql/thread/103abb3a-2f02-45c4-a4ba-392652c71f70"/></typeparam>
        /// <param name="paramName">The stored procedure parameter name</param>
        /// <param name="paramValues">The enumerable of <code>DTO</code> objects</param>
        /// <returns></returns>
        public static CriterionNVT Enumerable<DTO>(string paramName, IEnumerable<DTO> paramValues)
        {
            var integers = new DataTable();

            // get DTO type properties
            var dtoType = typeof (DTO);
            var properties = dtoType
                .GetProperties()
                .Where(property => property.CanRead)
                .ToList();

            properties
                .ForEach(property =>
                             {
                                 var type = property.PropertyType;
                                 integers.Columns
                                     .Add(property.Name,
                                          type.IsGenericType &&
                                          type.GetGenericTypeDefinition() == typeof (Nullable<>)
                                              ? property.PropertyType.GetGenericArguments()[0]
                                              : property.PropertyType);
                             });

            paramValues
                .ForEach(entry =>
                             {
                                 var row = integers.NewRow();
                                 properties.ForEach(property =>
                                                        {
                                                            var value = property.GetValue(entry, null);
                                                            row[property.Name] = value ?? DBNull.Value;
                                                        });
                                 integers.Rows.Add(row);
                             });
            return Structured(paramName, integers, string.Format("udtt_{0}", dtoType.Name));
        }

        /// <summary>
        /// Applicable only to ADO backend and only for M$ Sql Server 2008 (or greater?)
        /// Also, if you're having doubts about performance <see cref="http://www.sqlskills.com/BLOGS/BOBB/post/TVPs-and-plan-compilation-the-reprise.aspx"/>
        /// </summary>
        /// <param name="paramName">The parameter name</param>
        /// <param name="dataTable">The data table values</param>
        /// <param name="dataTableName">The data table type's name</param>
        /// <returns></returns>
        private static CriterionNVT Structured(string paramName, DataTable dataTable, string dataTableName)
        {
            return new CriterionNVT(paramName, DbType.Xml, dataTable, dataTableName);
        }
    }
}