using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlDSL
{
    public class Executable<TDb, TCmd> : IQueriable, IInsertable
        where TCmd : IDbCommand
        //where TDb : IDbConnection
    {
        protected List<CriterionNVT> _whereClauses;
        private readonly IDatabaseEngine<TDb, TCmd> _engine;
        private readonly int _successfulReturnValue;
        private readonly int _commandTimeout;
        private readonly Action<DbException> _treatDbException;
        private readonly Func<Action<string>> _makeTimerMetricCollector;
        protected List<IDataRowExtractor> _selection;
        protected string _dbName;

        public Executable(string connectionString
                          , IEnumerable<CriterionNVT> whereClauses
                          , IEnumerable<IDataRowExtractor> selection
                          , IDatabaseEngine<TDb, TCmd> engine
                          , int successfulReturnValue
                          , int commandTimeout
                          , Action<DbException> treatDbException
                          , Func<Action<string>> makeTimerMetricCollector
            )
            : this(connectionString
                   , whereClauses
                   , engine
                   , successfulReturnValue
                   , commandTimeout
                   , treatDbException
                   , makeTimerMetricCollector)
        {
            this._selection = (selection ?? new IDataRowExtractor[] { }).ToList();
        }

        public Executable(string connectionString
                          , IEnumerable<CriterionNVT> whereClauses
                          , IDatabaseEngine<TDb, TCmd> engine
                          , int successfulReturnValue
                          , int commandTimeout
                          , Action<DbException> treatDbException
                          , Func<Action<string>> makeTimerMetricCollector)
        {
            _dbName = connectionString;
            this._whereClauses = (whereClauses ?? new CriterionNVT[] { }).ToList();
            _engine = engine;
            _successfulReturnValue = successfulReturnValue;
            _commandTimeout = commandTimeout;
            _treatDbException = treatDbException;
            _makeTimerMetricCollector = makeTimerMetricCollector ?? (() => (name => { /* happy budda void */ }));
        }

        private int CommandTimeout(int? timeout)
        {
            return timeout.HasValue ? timeout.Value : _commandTimeout;
        }

        protected TDb GetDatabase()
        {
            return _engine.GetDatabase(DbConnectString);
        }

        protected int InvokeNonQuery(TDb db, TCmd dbCommand)
        {
            return _engine.InvokeNonQuery(db, dbCommand);
        }

        protected void SetSqlCmdParam(TDb db, TCmd dbCommand, CriterionNVT q)
        {
            _engine.SetSqlCmdParam(db, dbCommand, q);
        }

        protected Func<IDataReader> ExecuteReader(TDb db, TCmd dbCommand)
        {
            return _engine.ExecuteReader(db, dbCommand);
        }

        protected TCmd MakeSqlCommand(TDb db, string sql, int? timeOut = null)
        {
            return _engine.MakeSqlCommand(db, sql, CommandTimeout(timeOut));
        }

        protected TCmd MakeStoredProcCommand(TDb db, string storedProcedure, int? timeOut = null)
        {
            return _engine.MakeStoredProcCommand(db, storedProcedure, CommandTimeout(timeOut));
        }

        protected string DbConnectString
        {
            get { return _dbName; }
        }

        protected Func<IDataReader> SetSqlCmdParams(TDb db, TCmd dbCommand)
        {
            foreach (var q in _whereClauses)
            {
                SetSqlCmdParam(db, dbCommand, q);
            }
            return ExecuteReader(db, dbCommand);
        }

        private bool IsDatabaseConnectionDisposable()
        {
            var type = typeof (TDb);
            var asIDisposable = type.GetInterface(typeof (IDisposable).FullName);
            return asIDisposable != null;
        }


        private bool IsDbConnection(object connection)
        {
            return ImplementsInterface<IDbConnection>(connection);
        }

        private static bool ImplementsInterface<T>(object lObjet)
        {
            var type = lObjet.GetType();
            var asTtype = type.GetInterface(typeof(T).FullName);
            return asTtype != null;
        }


        protected void ExecuteQueryAndProcessRows(string aMetricName
                                                  , Converter<TDb, TCmd> dbCommandMaker
                                                  , params Action<IRow>[] entryProcessors)
        {
            bool failed = false;
            var collectMetric = _makeTimerMetricCollector();
            var db = default(TDb);
            try
            {
                if (IsDatabaseConnectionDisposable())
                {
                    db = GetDatabase();
                    using (db as IDisposable)
                    {
                        var dbCommand = dbCommandMaker(db);
                        var makeDataReader = SetSqlCmdParams(db, dbCommand);
                        ProcessReplyRows(entryProcessors, makeDataReader);
                    }
                }
                else
                {
                    db = GetDatabase();
                    var dbCommand = dbCommandMaker(db);
                    var makeDataReader = SetSqlCmdParams(db, dbCommand);
                    ProcessReplyRows(entryProcessors, makeDataReader);
                }
            }
            catch (DbException sqlx)
            {
                failed = true;
                _treatDbException(sqlx);
            }
            finally
            {
                if (!Equals(db, default(TDb)) && ImplementsInterface<IDbConnection>(db))
                    (db as IDbConnection).Close();
                var fullMetricName = string.Format("SELECT.{0}{1}"
                                                   , aMetricName.Contains(" ") ? "ad_hoc_query" : aMetricName
                                                   , failed ? ".failed" : string.Empty);
                collectMetric(fullMetricName);
            }
        }

        protected List<IRow> QueryExecutorProcess(string aMetricName, Converter<TDb, TCmd> dbCommandMaker)
        {
            var results = new List<IRow>();
            ExecuteQueryAndProcessRows(aMetricName, dbCommandMaker, results.Add);
            return results;
        }

        protected Tuple<List<T1>, List<T2>>
            QueryExecutorProcess<T1, T2>(string aMetricName
                                         , Converter<TDb, TCmd> dbCommandMaker
                                         , Func<IRow, T1> processor1
                                         , Func<IRow, T2> processor2)
        {
            var resultSet1 = new List<T1>();
            var resultSet2 = new List<T2>();
            ExecuteQueryAndProcessRows(aMetricName
                                       , dbCommandMaker
                                       , r1 => resultSet1.Add(processor1(r1))
                                       , r2 => resultSet2.Add(processor2(r2)));
            return new Tuple<List<T1>, List<T2>>(resultSet1, resultSet2);
        }

        protected Tuple<List<T1>, List<T2>, List<T3>>
            QueryExecutorProcess<T1, T2, T3>(string aMetricName
                                             , Converter<TDb, TCmd> dbCommandMaker
                                             , Func<IRow, T1> processor1
                                             , Func<IRow, T2> processor2
                                             , Func<IRow, T3> processor3)
        {
            var resultSet1 = new List<T1>();
            var resultSet2 = new List<T2>();
            var resultSet3 = new List<T3>();
            ExecuteQueryAndProcessRows(aMetricName
                                       , dbCommandMaker
                                       , r1 => resultSet1.Add(processor1(r1))
                                       , r2 => resultSet2.Add(processor2(r2))
                                       , r3 => resultSet3.Add(processor3(r3)));
            return new Tuple<List<T1>, List<T2>, List<T3>>(resultSet1, resultSet2, resultSet3);
        }

        protected Tuple<List<T1>, List<T2>, List<T3>, List<T4>>
            QueryExecutorProcess<T1, T2, T3, T4>(string aMetricName
                                                 , Converter<TDb, TCmd> dbCommandMaker
                                                 , Func<IRow, T1> processor1
                                                 , Func<IRow, T2> processor2
                                                 , Func<IRow, T3> processor3
                                                 , Func<IRow, T4> processor4)
        {
            var resultSet1 = new List<T1>();
            var resultSet2 = new List<T2>();
            var resultSet3 = new List<T3>();
            var resultSet4 = new List<T4>();
            ExecuteQueryAndProcessRows(aMetricName
                                       , dbCommandMaker
                                       , r1 => resultSet1.Add(processor1(r1))
                                       , r2 => resultSet2.Add(processor2(r2))
                                       , r3 => resultSet3.Add(processor3(r3))
                                       , r4 => resultSet4.Add(processor4(r4)));
            return new Tuple<List<T1>, List<T2>, List<T3>, List<T4>>(resultSet1, resultSet2, resultSet3, resultSet4);
        }

        protected Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>
            QueryExecutorProcess<T1, T2, T3, T4, T5>(string aMetricName
                                                     , Converter<TDb, TCmd> dbCommandMaker
                                                     , Func<IRow, T1> processor1
                                                     , Func<IRow, T2> processor2
                                                     , Func<IRow, T3> processor3
                                                     , Func<IRow, T4> processor4
                                                     , Func<IRow, T5> processor5)
        {
            var resultSet1 = new List<T1>();
            var resultSet2 = new List<T2>();
            var resultSet3 = new List<T3>();
            var resultSet4 = new List<T4>();
            var resultSet5 = new List<T5>();
            ExecuteQueryAndProcessRows(aMetricName
                                       , dbCommandMaker
                                       , r1 => resultSet1.Add(processor1(r1))
                                       , r2 => resultSet2.Add(processor2(r2))
                                       , r3 => resultSet3.Add(processor3(r3))
                                       , r4 => resultSet4.Add(processor4(r4))
                                       , r5 => resultSet5.Add(processor5(r5)));
            return new Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>(resultSet1, resultSet2, resultSet3,
                                                                               resultSet4, resultSet5);
        }

        protected Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>
            QueryExecutorProcess<T1, T2, T3, T4, T5, T6>(string storedProcedure
                                                         , Converter<TDb, TCmd> dbCommandMaker
                                                         , Func<IRow, T1> processor1
                                                         , Func<IRow, T2> processor2
                                                         , Func<IRow, T3> processor3
                                                         , Func<IRow, T4> processor4
                                                         , Func<IRow, T5> processor5
                                                         , Func<IRow, T6> processor6)
        {
            var resultSet1 = new List<T1>();
            var resultSet2 = new List<T2>();
            var resultSet3 = new List<T3>();
            var resultSet4 = new List<T4>();
            var resultSet5 = new List<T5>();
            var resultSet6 = new List<T6>();
            ExecuteQueryAndProcessRows(storedProcedure
                                       , dbCommandMaker
                                       , r1 => resultSet1.Add(processor1(r1))
                                       , r2 => resultSet2.Add(processor2(r2))
                                       , r3 => resultSet3.Add(processor3(r3))
                                       , r4 => resultSet4.Add(processor4(r4))
                                       , r5 => resultSet5.Add(processor5(r5))
                                       , r6 => resultSet6.Add(processor6(r6)));
            return new Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>(resultSet1
                                                                                         , resultSet2
                                                                                         , resultSet3
                                                                                         , resultSet4
                                                                                         , resultSet5
                                                                                         , resultSet6);
        }

        protected void InsertDeleteUpdateExecutorProcess(Converter<TDb, TCmd> dbCommandMaker
                                                         , string code
                                                         , int successValue)
        {
            bool failed = false;
            var collectMetric = _makeTimerMetricCollector();
            var db = default(TDb);
            try
            {
                var ret = successValue + 1; // AKA: ERROR!
                if (IsDatabaseConnectionDisposable())
                {
                    db = GetDatabase();
                    using (db as IDisposable)
                    {
                        var dbCommand = dbCommandMaker(db);
                        foreach (var q in _whereClauses)
                        {
                            SetSqlCmdParam(db, dbCommand, q);
                        }
                        ret = InvokeNonQuery(db, dbCommand);
                    }
                }
                else
                {
                    db = GetDatabase();
                    var dbCommand = dbCommandMaker(db);
                    foreach (var q in _whereClauses)
                    {
                        SetSqlCmdParam(db, dbCommand, q);
                    }
                    ret = InvokeNonQuery(db, dbCommand);
                }
                if (ret != successValue)
                {
                    throw new DataException(code + " return value (" + ret + ")");
                }
            }
            catch (DbException sqlx)
            {
                failed = true;
                _treatDbException(sqlx);
            }
            finally
            {
                if (!Equals(db, default(TDb)) && ImplementsInterface<IDbConnection>(db))
                    (db as IDbConnection).Close();
                var fullMetricName = string.Format("INSERT.{0}{1}"
                                                   , code.Contains(" ") ? "ad_hoc_statement" : code
                                                   , failed ? ".failed" : string.Empty);
                collectMetric(fullMetricName);
            }
        }

        protected Row GetFullRow(IDataReader reader)
        {
            var result = new Hashtable();
            if (_selection.Count > 0)
                foreach (var rowExtractor in _selection)
                    rowExtractor.Extract(reader, result);
            else
            {
                // blank selection
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Type type = reader.GetFieldType(i);
                    string colName = reader.GetName(i);
                    switch (type.FullName.ToLower())
                    {
                        case "system.int32":
                            Value.Int(colName).Extract(reader, result);
                            break;
                        case "system.string":
                            Value.String(colName).Extract(reader, result);
                            break;
                        case "system.double":
                        case "system.real":
                        case "system.float":
                        case "system.single":
                            Value.Double(colName).Extract(reader, result);
                            break;
                        case "system.datetime":
                            Value.DateTime(colName).Extract(reader, result);
                            break;
                        case "system.guid":
                            Value.Guid(colName).Extract(reader, result);
                            break;
                        case "system.decimal":
                            Value.Decimal(colName).Extract(reader, result);
                            break;
                        case "system.boolean":
                            Value.Bool(colName).Extract(reader, result);
                            break;
                        case "system.byte[]":
                            Value.Binary(colName).Extract(reader, result);
                            break;
                        case "system.byte":
                            Value.Int(colName).Extract(reader, result);
                            break;
                        case "system.int64":
                            Value.Int64(colName).Extract(reader, result);
                            break;
                        default:
                            result.Add(colName, reader[colName]);
                            break;
                    }
                }
            }
            return new Row(result);
        }

        protected void ProcessReplyRows(IEnumerable<Action<IRow>> entryProcessors, Func<IDataReader> makeDataReader)
        {
            using (var reader = makeDataReader())
            {
                var notFirstResultSet = false;
                foreach (var entryProcessor in entryProcessors)
                {
                    if (notFirstResultSet)
                        reader.NextResult();
                    else
                        notFirstResultSet = true;

                    while (reader.Read())
                    {
                        var entry = GetFullRow(reader);
                        entryProcessor(entry);
                    }
                }
            }
        }

        #region IQueriable Members
        public List<IRow> ExecuteSql(string sql)
        {
            return ExecuteSql(sql, _commandTimeout);
        }

        public List<IRow> ExecuteSql(string sql, int commandTimeout)
        {
            return QueryExecutorProcess(sql
                                        , db => MakeSqlCommand(db
                                                               , sql
                                                               , CommandTimeout(commandTimeout)));
        }

        public Tuple<List<T1>, List<T2>> ExecuteSql<T1, T2>(string sql
                                                            , Func<IRow, T1> extractResultSetT1
                                                            , Func<IRow, T2> extractResultSetT2)
        {
            return QueryExecutorProcess(sql
                                        , db => MakeSqlCommand(db, sql)
                                        , extractResultSetT1
                                        , extractResultSetT2);
        }

        public Tuple<List<T1>, List<T2>, List<T3>> ExecuteSql<T1, T2, T3>(string sql
                                                                          , Func<IRow, T1> extractResultSetT1
                                                                          , Func<IRow, T2> extractResultSetT2
                                                                          , Func<IRow, T3> extractResultSetT3)
        {
            return QueryExecutorProcess(sql
                                        , db => MakeSqlCommand(db, sql)
                                        , extractResultSetT1
                                        , extractResultSetT2
                                        , extractResultSetT3);
        }

        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>>
            ExecuteSql<T1, T2, T3, T4>(string sql
                                       , Func<IRow, T1> extractResultSetT1
                                       , Func<IRow, T2> extractResultSetT2
                                       , Func<IRow, T3> extractResultSetT3
                                       , Func<IRow, T4> extractResultSetT4)
        {
            return QueryExecutorProcess(sql
                                        , db => MakeSqlCommand(db, sql)
                                        , extractResultSetT1
                                        , extractResultSetT2
                                        , extractResultSetT3
                                        , extractResultSetT4);
        }

        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>
            ExecuteSql<T1, T2, T3, T4, T5>(string sql
                                           , Func<IRow, T1> extractResultSetT1
                                           , Func<IRow, T2> extractResultSetT2
                                           , Func<IRow, T3> extractResultSetT3
                                           , Func<IRow, T4> extractResultSetT4
                                           , Func<IRow, T5> extractResultSetT5)
        {
            return QueryExecutorProcess(sql
                                        , db => MakeSqlCommand(db, sql)
                                        , extractResultSetT1
                                        , extractResultSetT2
                                        , extractResultSetT3
                                        , extractResultSetT4
                                        , extractResultSetT5);
        }

        public Tuple<List<T1>, List<T2>> ExecuteQuery<T1, T2>(string storedProcedure
                                                              , Func<IRow, T1> extractResultSetT1
                                                              , Func<IRow, T2> extractResultSetT2)
        {
            return QueryExecutorProcess(storedProcedure
                                        , db => MakeStoredProcCommand(db, storedProcedure)
                                        , extractResultSetT1
                                        , extractResultSetT2);
        }

        public Tuple<List<T1>, List<T2>, List<T3>> ExecuteQuery<T1, T2, T3>(string storedProcedure
                                                                            , Func<IRow, T1> extractResultSetT1
                                                                            , Func<IRow, T2> extractResultSetT2
                                                                            , Func<IRow, T3> extractResultSetT3)
        {
            return QueryExecutorProcess(storedProcedure
                                        , db => MakeStoredProcCommand(db, storedProcedure)
                                        , extractResultSetT1
                                        , extractResultSetT2
                                        ,extractResultSetT3);
        }

        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>>
            ExecuteQuery<T1, T2, T3, T4>(string storedProcedure
                                         , Func<IRow, T1> extractResultSetT1
                                         , Func<IRow, T2> extractResultSetT2
                                         , Func<IRow, T3> extractResultSetT3
                                         , Func<IRow, T4> extractResultSetT4)
        {
            return QueryExecutorProcess(storedProcedure
                                        , db => MakeStoredProcCommand(db, storedProcedure)
                                        , extractResultSetT1
                                        , extractResultSetT2
                                        , extractResultSetT3
                                        , extractResultSetT4);
        }

        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>
            ExecuteQuery<T1, T2, T3, T4, T5>(string storedProcedure
                                             , Func<IRow, T1> extractResultSetT1
                                             , Func<IRow, T2> extractResultSetT2
                                             , Func<IRow, T3> extractResultSetT3
                                             , Func<IRow, T4> extractResultSetT4
                                             , Func<IRow, T5> extractResultSetT5)
        {
            return QueryExecutorProcess(storedProcedure
                                        , db => MakeStoredProcCommand(db, storedProcedure)
                                        , extractResultSetT1
                                        , extractResultSetT2
                                        , extractResultSetT3
                                        , extractResultSetT4
                                        , extractResultSetT5);
        }

        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>
            ExecuteQuery<T1, T2, T3, T4, T5, T6>(string storedProcedure
                                                 , Func<IRow, T1> extractResultSetT1
                                                 , Func<IRow, T2> extractResultSetT2
                                                 , Func<IRow, T3> extractResultSetT3
                                                 , Func<IRow, T4> extractResultSetT4
                                                 , Func<IRow, T5> extractResultSetT5
                                                 , Func<IRow, T6> extractResultSetT6)
        {
            return QueryExecutorProcess(storedProcedure
                                        , db => MakeStoredProcCommand(db, storedProcedure)
                                        , extractResultSetT1
                                        , extractResultSetT2
                                        , extractResultSetT3
                                        , extractResultSetT4
                                        , extractResultSetT5
                                        , extractResultSetT6);
        }

        public List<IRow> ExecuteQuery(string storedProcedure)
        {
            return ExecuteQuery(storedProcedure, _commandTimeout);
        }

        public List<IRow> ExecuteQuery(string storedProcedure, int commandTimeout)
        {
            return QueryExecutorProcess(storedProcedure
                                        , db => MakeStoredProcCommand(db
                                                                      , storedProcedure
                                                                      , commandTimeout));
        }
        #endregion

        #region IInsertable Members
        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql)
        {
            ExecuteInsertOrDeleteOrUpdateSQL(sql, _successfulReturnValue);
        }

        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue)
        {
            ExecuteInsertOrDeleteOrUpdateSQL(sql, successValue, _commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue, int commandTimeout)
        {
            InsertDeleteUpdateExecutorProcess(db => MakeSqlCommand(db, sql, commandTimeout)
                                              , sql
                                              , successValue);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue)
        {
            ExecuteInsertOrDeleteOrUpdate(storedProcedure, successValue, _commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue, int commandTimeout)
        {
            InsertDeleteUpdateExecutorProcess(db => MakeStoredProcCommand(db, storedProcedure, CommandTimeout(commandTimeout))
                                              , storedProcedure
                                              , successValue);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure)
        {
            ExecuteInsertOrDeleteOrUpdate(storedProcedure, _successfulReturnValue);
        }
        #endregion IInsertable Members

    }
}