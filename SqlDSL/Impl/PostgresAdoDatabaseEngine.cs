using System;
using System.Data;
using Npgsql;

namespace SqlDSL.Impl
{
    /// <summary>
    /// This is currently almost the same as <see cref="MsSsAdoDatabaseEngine"/>.
    /// </summary>
    public class PostgresAdoDatabaseEngine : IDatabaseEngine<NpgsqlConnection, NpgsqlCommand>
    {
        public int InvokeNonQuery(NpgsqlConnection db, NpgsqlCommand dbCommand)
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

        public Func<IDataReader> ExecuteReader(NpgsqlConnection db, NpgsqlCommand dbCommand)
        {
            return () => dbCommand.ExecuteReader();
        }

        public NpgsqlCommand MakeSqlCommand(NpgsqlConnection db, string sql, int? timeOut = new int?())
        {
            var cmd = new NpgsqlCommand(sql, db);
            if (timeOut.HasValue)
                cmd.CommandTimeout = timeOut.Value;
            return cmd;
        }

        public NpgsqlCommand MakeStoredProcCommand(NpgsqlConnection db, string storedProcedure, int? timeOut = new int?())
        {
            var cmd = new NpgsqlCommand(storedProcedure, db)
                          {
                              CommandType = CommandType.StoredProcedure
                          };
            if (timeOut.HasValue)
                cmd.CommandTimeout = timeOut.Value;
            return cmd;
        }

        public void SetSqlCmdParam(NpgsqlConnection db, NpgsqlCommand dbCommand, CriterionNVT q)
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

        public NpgsqlConnection GetDatabase(string dbConnectString)
        {
            var connection = new NpgsqlConnection(dbConnectString);
            connection.Open();
            return connection;
        }
    }
}