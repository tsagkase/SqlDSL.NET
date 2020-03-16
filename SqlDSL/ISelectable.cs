using System.Collections;
using System.Data;

namespace SqlDSL
{
    public interface ISelectable
    {
        IQueriable Select(params IDataRowExtractor[] selection);
        IQueriable SelectAll();
    }

    public interface IDataRowExtractor
    {
        void Extract(IDataReader row, IDictionary results);
    }
}