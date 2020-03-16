namespace SqlDSL
{
    public interface IClausable
    {
        IEitherWritableOrSelectable Where(params CriterionNVT[] clauses);
        bool IsIEnumerableSupported();
    }
}