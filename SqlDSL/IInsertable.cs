namespace SqlDSL
{
    public interface IInsertable
    {
        void ExecuteInsertOrDeleteOrUpdate(string storedProcedure);
        void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue);
        void ExecuteInsertOrDeleteOrUpdate(string storedProcedure, int successValue, int commandTimeout);
        void ExecuteInsertOrDeleteOrUpdateSQL(string sql);
        void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue);
        void ExecuteInsertOrDeleteOrUpdateSQL(string sql, int successValue, int commandTimeout);
    }
}