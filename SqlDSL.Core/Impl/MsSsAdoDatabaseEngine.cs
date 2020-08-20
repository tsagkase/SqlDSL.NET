using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlDSL.Core.Impl
{
    public class MsSsAdoDatabaseEngine : IDatabaseEngine<SqlConnection, SqlCommand>
    {
        public SqlConnection GetDatabase(string dbConnectString)
        {
            var connection = new SqlConnection(dbConnectString);
            connection.Open();
            return connection;
        }

        public int InvokeNonQuery(SqlConnection db, SqlCommand dbCommand)
        {
            var returnParameter = dbCommand.Parameters.Add("RetVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            dbCommand.ExecuteNonQuery();
            return (int) returnParameter.Value;
        }

        public Func<IDataReader> ExecuteReader(SqlConnection ignored, SqlCommand dbCommand)
        {
            return dbCommand.ExecuteReader;
        }

        public SqlCommand MakeSqlCommand(SqlConnection db, string sql, int? timeOut = null)
        {
            var cmd = new SqlCommand(sql, db);
            if (timeOut.HasValue)
                cmd.CommandTimeout = timeOut.Value;
            return cmd;
        }

        public SqlCommand MakeStoredProcCommand(SqlConnection db, string storedProcedure, int? timeOut = null)
        {
            var cmd = new SqlCommand(storedProcedure, db) {CommandType = CommandType.StoredProcedure};
            if (timeOut.HasValue)
                cmd.CommandTimeout = timeOut.Value;
            return cmd;
        }

        public void SetSqlCmdParam(SqlConnection ignored, SqlCommand dbCommand, CriterionNVT q)
        {
            if (string.IsNullOrEmpty(q.TypeName))
                dbCommand.Parameters.AddWithValue(q.Name, q.Value ?? DBNull.Value);
            else
            {
                var param = dbCommand.Parameters.Add(new SqlParameter(q.Name, q.Value));
                param.Value = q.Value ?? DBNull.Value;
                param.TypeName = q.TypeName;
            }
        }
    }
}
