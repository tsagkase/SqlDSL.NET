using System;
using System.Data;

namespace SqlDSL.Core
{
    public interface IDatabaseEngine<TDb, TCmd>
        where TCmd : IDbCommand
    {
        int InvokeNonQuery(TDb db, TCmd dbCommand);
        Func<IDataReader> ExecuteReader(TDb db, TCmd dbCommand);
        TCmd MakeSqlCommand(TDb db, string sql, int? timeOut = null);
        TCmd MakeStoredProcCommand(TDb db, string storedProcedure, int? timeOut = null);
        void SetSqlCmdParam(TDb db, TCmd dbCommand, CriterionNVT q);
        TDb GetDatabase(string dbConnectString);
    }
}