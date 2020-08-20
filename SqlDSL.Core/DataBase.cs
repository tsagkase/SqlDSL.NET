using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using SqlDSL.Core.Impl;

namespace SqlDSL.Core
{
    public class DataBase : IClausable, ISelectable, IInsertable
    {
        private const int DFLT_CMD_TIMEOUT = 30;
        private const int DFLT_SUCCESS_RETURN = 0;
        protected readonly string _dbConnectionString;
        private readonly int _successfulReturnValue;
        private readonly int _commandTimeout;
        private readonly IDatabaseEngineFactory _databaseEngineFactory;

        public DataBase(string dbConnectionString
                           , Func<Action<string>> makeTimerMetricCollector = null
                           , int successfulReturnValue = DFLT_SUCCESS_RETURN
                           , int commandTimeout = DFLT_CMD_TIMEOUT
                           , IDatabaseEngineFactory databaseEngineFactory = null
                           , Action<DbException> treatDbException = null)
        {
            this._dbConnectionString = dbConnectionString;
            _successfulReturnValue = successfulReturnValue;
            _commandTimeout = commandTimeout;
            _databaseEngineFactory = databaseEngineFactory ?? DefaultDatabaseEngineFactory(makeTimerMetricCollector);
            _databaseEngineFactory.CommandTimeout = commandTimeout;
            _databaseEngineFactory.SuccessfulReturnValue = successfulReturnValue;
            _databaseEngineFactory.DbExceptionHandler = treatDbException ?? Rethrow;
        }

        private static void Rethrow(DbException dbx)
        {
            throw dbx;
        }

        private static DatabaseEngineFactory DefaultDatabaseEngineFactory(Func<Action<string>> makeTimerMetricCollector)
        {
            return new DatabaseEngineFactory("SqlServer2008.Ado", makeTimerMetricCollector);
        }

        #region IClausable Members
        public IEitherWritableOrSelectable Where(params CriterionNVT[] clauses)
        {
            return new Selectable(_dbConnectionString
                                  , _databaseEngineFactory
                                  , _successfulReturnValue
                                  , _commandTimeout
                                  , clauses);
        }

        public bool IsIEnumerableSupported()
        {
            // TODO: _assumeDaab is not enough a constraint! we also require MS Sql Server 2008... for now it will do!
            return _databaseEngineFactory.IsIEnumerableSupported();
        }

        public IEitherWritableOrSelectable Where(IEnumerable<CriterionNVT> clauses)
        {
            return Where(clauses.ToArray());
        }
        #endregion

        #region ISelectable Members
        public IQueriable Select(params IDataRowExtractor[] selection)
        {
            return new Selectable(_dbConnectionString
                                  , _databaseEngineFactory
                                  , _successfulReturnValue
                                  , _commandTimeout
                                  , new CriterionNVT[] {})
                .Select(selection);
        }

        public IQueriable SelectAll()
        {
            return new Selectable(_dbConnectionString
                                  , _databaseEngineFactory
                                  , _successfulReturnValue
                                  , _commandTimeout
                                  , new CriterionNVT[] { })
                .SelectAll();
        }
        #endregion

        #region IInsertable Members
        private IInsertable GetNoCriteriaExecutable()
        {
            var criteria = new CriterionNVT[] {};
            return _databaseEngineFactory.MakeInsertable(_dbConnectionString, criteria);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue)
        {
            ExecuteInsertOrDeleteOrUpdate(storedProcedure, successValue, _commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue, int commandTimeout)
        {
            GetNoCriteriaExecutable().ExecuteInsertOrDeleteOrUpdate(storedProcedure, successValue, commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue)
        {
            ExecuteInsertOrDeleteOrUpdateSQL(sql, successValue, _commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue, int commandTimeout)
        {
            GetNoCriteriaExecutable().ExecuteInsertOrDeleteOrUpdateSQL(sql, successValue, commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql)
        {
            ExecuteInsertOrDeleteOrUpdateSQL(sql, _successfulReturnValue, _commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure)
        {
            ExecuteInsertOrDeleteOrUpdate(storedProcedure, _successfulReturnValue, _commandTimeout);
        }
        #endregion
    }
}