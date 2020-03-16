namespace SqlDSL
{
    public class Selectable : IEitherWritableOrSelectable
    {
        private readonly CriterionNVT[] criteria;
        private readonly string dbConnection;
        private readonly IDatabaseEngineFactory _databaseEngineFactory;
        private readonly int _successfulReturnValue;
        private readonly int _commandTimeout;

        public Selectable(string connectionString
                          , IDatabaseEngineFactory databaseEngineFactory
                          , int successfulReturnValue
                          , int commandTimeout
                          , params CriterionNVT[] clauses)
        {
            dbConnection = connectionString;
            _databaseEngineFactory = databaseEngineFactory;
            _successfulReturnValue = successfulReturnValue;
            _commandTimeout = commandTimeout;
            criteria = clauses;
        }

        #region ISelectable Members
        public IQueriable Select(params IDataRowExtractor[] selection)
        {
            return _databaseEngineFactory.MakeQueriable(dbConnection, criteria, selection);
        }

        public IQueriable SelectAll()
        {
            return _databaseEngineFactory.MakeQueriable(dbConnection, criteria);
        }
        #endregion

        #region IInsertable Members
        private IInsertable GetExecutable()
        {
            return _databaseEngineFactory.MakeInsertable(dbConnection, criteria);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue)
        {
            ExecuteInsertOrDeleteOrUpdate(storedProcedure, successValue, _commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue, int commandTimeout)
        {
            GetExecutable().ExecuteInsertOrDeleteOrUpdate(storedProcedure, successValue, commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue)
        {
            ExecuteInsertOrDeleteOrUpdateSQL(sql, successValue, _commandTimeout);
        }

        public void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue, int commandTimeout)
        {
            GetExecutable().ExecuteInsertOrDeleteOrUpdateSQL(sql, successValue, commandTimeout);
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