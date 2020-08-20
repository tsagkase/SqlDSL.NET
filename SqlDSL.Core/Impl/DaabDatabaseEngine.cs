using System;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace SqlDSL.Core.Impl
{
    public class DaabDatabaseEngine : IDatabaseEngine<Database, DbCommand>
    {
        //public Executable(string connectionString,
        //                  CriterionNVT[] whereClauses,
        //                  IDataRowExtractor[] selection)
        //    : base(connectionString, whereClauses, selection)
        //{ }

        //public Executable(string connectionString, CriterionNVT[] whereClauses)
        //    : base(connectionString, whereClauses)
        //{ }

        public Database GetDatabase(string dbConnectString)
        {
            return DatabaseFactory.CreateDatabase(dbConnectString);
        }

        public int InvokeNonQuery(Database db, DbCommand dbCommand)
        {
            db.AddOutParameter(dbCommand, "RetVal", DbType.Int32, 1);
            dbCommand.Parameters["@RetVal"].Direction = ParameterDirection.ReturnValue;
            db.ExecuteNonQuery(dbCommand);
            return (int)dbCommand.Parameters["@RetVal"].Value;
        }

        public Func<IDataReader> ExecuteReader(Database db, DbCommand dbCommand)
        {
            return () => db.ExecuteReader(dbCommand);
        }

        public DbCommand MakeSqlCommand(Database db, string sql, int? timeOut = null)
        {
            var dbCommand = db.GetSqlStringCommand(sql);
            if (timeOut.HasValue)
                dbCommand.CommandTimeout = timeOut.Value;
            return dbCommand;
        }

        public DbCommand MakeStoredProcCommand(Database db, string storedProcedure, int? timeOut = null)
        {
            var dbCommand = db.GetStoredProcCommand(storedProcedure);
            if (timeOut.HasValue)
                dbCommand.CommandTimeout = timeOut.Value;
            return dbCommand;
        }

        public void SetSqlCmdParam(Database db, DbCommand dbCommand, CriterionNVT q)
        {
            db.AddInParameter(dbCommand, q.Name, q.Type, q.Value ?? DBNull.Value);
        }
    }
}