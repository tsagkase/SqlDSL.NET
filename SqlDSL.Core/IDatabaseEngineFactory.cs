using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SqlDSL.Core
{
    public interface IDatabaseEngineFactory
    {
        bool IsIEnumerableSupported();

        IQueriable MakeQueriable(string connection, IEnumerable<CriterionNVT> criteria);
        IQueriable MakeQueriable(string connection, IEnumerable<CriterionNVT> criteria, IEnumerable<IDataRowExtractor> selection);
        IInsertable MakeInsertable(string connection, IEnumerable<CriterionNVT> criteria);

        int SuccessfulReturnValue { set; }
        int CommandTimeout { set; }
        Action<DbException> DbExceptionHandler { get; set; }
    }
}