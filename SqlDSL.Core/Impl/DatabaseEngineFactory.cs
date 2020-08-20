using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Npgsql;

namespace SqlDSL.Core.Impl
{
    public class DatabaseEngineFactory : IDatabaseEngineFactory
    {
        private const int DFLT_SUCCESS_RETURN = 0;
        private const int DFLT_CMD_TIMEOUT = 30;
        public int SuccessfulReturnValue { get; set; }
        public int CommandTimeout { get; set; }
        public Action<DbException> DbExceptionHandler { get; set; }
        private readonly string _driver;
        private readonly Func<Action<string>> _makeTimerMetricCollector;

        public DatabaseEngineFactory(string driver, Func<Action<string>> makeTimerMetricCollector)
            : this(driver
                   , DFLT_SUCCESS_RETURN
                   , DFLT_CMD_TIMEOUT
                   , TreatDbException
                   , makeTimerMetricCollector)
        {
            _driver = driver;
        }

        public DatabaseEngineFactory(string driver
                                     , int successfulReturnValue
                                     , int commandTimeout
                                     , Action<DbException> treatDbException
                                     , Func<Action<string>> makeTimerMetricCollector)
        {
            SuccessfulReturnValue = successfulReturnValue;
            CommandTimeout = commandTimeout;
            _driver = driver;
            _makeTimerMetricCollector = makeTimerMetricCollector;
            DbExceptionHandler = treatDbException;
        }

        protected static void TreatDbException(DbException sqlx)
        {
            if (IsBusinessRuleException(sqlx))
            {
                //Implementation of BusinessRulesException
                throw new Exception(ExtractErrorCode(sqlx), sqlx);
            }
            throw new Exception(sqlx.ToString(), sqlx);
        }

        private static bool IsBusinessRuleException(DbException sqlx)
        {
            string msg = sqlx.Message.ToUpper();
            return msg.Split(':')[0].Trim().Equals("BRE");
        }

        private static string ExtractErrorCode(DbException sqlx)
        {
            string msg = sqlx.Message;
            return msg.Split(':')[1].Split(' ', '\t')[0];
        }

        private object Make(string connection, IEnumerable<CriterionNVT> criteria, IEnumerable<IDataRowExtractor> selection = null)
        {
            if (selection == null)
                selection = new IDataRowExtractor[] { };

            switch (_driver)
            {
                case "SqlServer2008":
                case "SqlServer2008.Ado":
                    return new Executable<SqlConnection, SqlCommand>(connection
                                                                     , criteria
                                                                     , selection
                                                                     , new MsSsAdoDatabaseEngine()
                                                                     , SuccessfulReturnValue
                                                                     , CommandTimeout
                                                                     , DbExceptionHandler
                                                                     , _makeTimerMetricCollector);
                case "SqlServer2005":
                case "SqlServer2005.Ado":
                    // TODO: Pass flag that no Enumerable permitted
                    return new Executable<SqlConnection, SqlCommand>(connection
                                                                     , criteria
                                                                     , selection
                                                                     , new MsSsAdoDatabaseEngine()
                                                                     , SuccessfulReturnValue
                                                                     , CommandTimeout
                                                                     , DbExceptionHandler
                                                                     , _makeTimerMetricCollector);
                case "PostgreSql":
                case "PostgreSql.Ado":
                    return new Executable<NpgsqlConnection, NpgsqlCommand>(connection
                                                                           , criteria
                                                                           , selection
                                                                           , new PostgresAdoDatabaseEngine()
                                                                           , SuccessfulReturnValue
                                                                           , CommandTimeout
                                                                           , DbExceptionHandler
                                                                           , _makeTimerMetricCollector);

                case "SqlServer":
                case "SqlServer.Daab":
                default:
                    return new Executable<Database, DbCommand>(connection
                                                               , criteria
                                                               , selection
                                                               , new DaabDatabaseEngine()
                                                               , SuccessfulReturnValue
                                                               , CommandTimeout
                                                               , DbExceptionHandler
                                                               , _makeTimerMetricCollector);
            }
        }

        public bool IsIEnumerableSupported()
        {
            switch (_driver)
            {
                case "SqlServer2008":
                case "SqlServer2008.Ado":
                    return true;
                case "SqlServer2005":
                case "SqlServer2005.Ado":
                case "PostgreSql":
                case "PostgreSql.Ado":
                case "SqlServer":
                case "SqlServer.Daab":
                default:
                    return false;
            }
        }

        public IQueriable MakeQueriable(string connection, IEnumerable<CriterionNVT> criteria)
        {
            return MakeQueriable(connection, criteria, new IDataRowExtractor[] {});
        }

        public IQueriable MakeQueriable(string connection, IEnumerable<CriterionNVT> criteria, IEnumerable<IDataRowExtractor> selection)
        {
            return Make(connection, criteria, selection) as IQueriable;
        }

        public IInsertable MakeInsertable(string connection, IEnumerable<CriterionNVT> criteria)
        {
            return Make(connection, criteria) as IInsertable;
        }
    }
}