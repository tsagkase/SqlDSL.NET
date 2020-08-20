using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace SqlDSL.Core.Impl
{
    public class OracleAdoDatabaseEngine : IDatabaseEngine<OracleConnection, OracleCommand>
    {
        public int InvokeNonQuery(OracleConnection db, OracleCommand dbCommand)
        {
            dbCommand.ExecuteNonQuery();
            return 0; //  dbCommand.ExecuteNonQuery() > 0 ? 0 : 1;
            // TODO: check return value for success!
            /*
            var returnParameter = dbCommand.Parameters.Add("RetVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            dbCommand.ExecuteNonQuery();
            return (int)returnParameter.Value;
             */
        }

        public Func<IDataReader> ExecuteReader(OracleConnection db, OracleCommand dbCommand)
        {
            return () => dbCommand.ExecuteReader();
        }

        public OracleCommand MakeSqlCommand(OracleConnection db, string sql, int? timeOut = new int?())
        {
            var cmd = new OracleCommand(sql, db);
            if (timeOut.HasValue)
                cmd.CommandTimeout = timeOut.Value;
            return cmd;
        }

        public OracleCommand MakeStoredProcCommand(OracleConnection db, string storedProcedure, int? timeOut = new int?())
        {
            var cmd = new OracleCommand(storedProcedure, db)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (timeOut.HasValue)
                cmd.CommandTimeout = timeOut.Value;
            return cmd;
        }

        public void SetSqlCmdParam(OracleConnection db, OracleCommand dbCommand, CriterionNVT q)
        {
            var parameter = dbCommand.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.DbType = q.Type;
            parameter.ParameterName = q.Name;
            if (q.Type == DbType.String && q.Value != null)
            {
                var value = q.Value as string;
                value = value.Replace("\0", string.Empty);
                parameter.Value = value;
            }
            else
                parameter.Value = q.Value ?? DBNull.Value;
            dbCommand.Parameters.Add(parameter);
        }

        public OracleConnection GetDatabase(string dbConnectString)
        {
            var connection = new OracleConnection(dbConnectString);
            connection.Open();
            return connection;
        }
    }
}
