using System;
using System.Collections.Generic;

namespace SqlDSL.Core
{
    public interface IQueriable
    {
        List<IRow> ExecuteSql(string sql);
        List<IRow> ExecuteSql(string sql, int commandTimeout);

        // the future
        Tuple<List<T1>, List<T2>> ExecuteSql<T1, T2>(string sql
                                                     , Func<IRow, T1> extractResultSetT1
                                                     , Func<IRow, T2> extractResultSetT2);

        Tuple<List<T1>, List<T2>, List<T3>> ExecuteSql<T1, T2, T3>(string sql
                                                                   , Func<IRow, T1> extractResultSetT1
                                                                   , Func<IRow, T2> extractResultSetT2
                                                                   , Func<IRow, T3> extractResultSetT3);

        Tuple<List<T1>, List<T2>, List<T3>, List<T4>>
            ExecuteSql<T1, T2, T3, T4>(string sql
                                       , Func<IRow, T1> extractResultSetT1
                                       , Func<IRow, T2> extractResultSetT2
                                       , Func<IRow, T3> extractResultSetT3
                                       , Func<IRow, T4> extractResultSetT4);

        Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>
            ExecuteSql<T1, T2, T3, T4, T5>(string sql
                                           , Func<IRow, T1> extractResultSetT1
                                           , Func<IRow, T2> extractResultSetT2
                                           , Func<IRow, T3> extractResultSetT3
                                           , Func<IRow, T4> extractResultSetT4
                                           , Func<IRow, T5> extractResultSetT5);


        List<IRow> ExecuteQuery(string storedProcedure);
        List<IRow> ExecuteQuery(string storedProcedure, int commandTimeout);
        Tuple<List<T1>, List<T2>> ExecuteQuery<T1, T2>(string storedProcedure
                                                       , Func<IRow, T1> extractResultSetT1
                                                       , Func<IRow, T2> extractResultSetT2);

        Tuple<List<T1>, List<T2>, List<T3>> ExecuteQuery<T1, T2, T3>(string storedProcedure
                                                                     , Func<IRow, T1> extractResultSetT1
                                                                     , Func<IRow, T2> extractResultSetT2
                                                                     , Func<IRow, T3> extractResultSetT3);

        Tuple<List<T1>, List<T2>, List<T3>, List<T4>>
            ExecuteQuery<T1, T2, T3, T4>(string storedProcedure
                                         , Func<IRow, T1> extractResultSetT1
                                         , Func<IRow, T2> extractResultSetT2
                                         , Func<IRow, T3> extractResultSetT3
                                         , Func<IRow, T4> extractResultSetT4);

        Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>
            ExecuteQuery<T1, T2, T3, T4, T5>(string storedProcedure
                                             , Func<IRow, T1> extractResultSetT1
                                             , Func<IRow, T2> extractResultSetT2
                                             , Func<IRow, T3> extractResultSetT3
                                             , Func<IRow, T4> extractResultSetT4
                                             , Func<IRow, T5> extractResultSetT5);

        Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>
            ExecuteQuery<T1, T2, T3, T4, T5, T6>(string storedProcedure
                                                 , Func<IRow, T1> extractResultSetT1
                                                 , Func<IRow, T2> extractResultSetT2
                                                 , Func<IRow, T3> extractResultSetT3
                                                 , Func<IRow, T4> extractResultSetT4
                                                 , Func<IRow, T5> extractResultSetT5
                                                 , Func<IRow, T6> extractResultSetT6);
    }
}