namespace SqlDSL.Core
{
    public interface IClausable
    {
        IEitherWritableOrSelectable Where(params CriterionNVT[] clauses);
        bool IsIEnumerableSupported();
    }
}