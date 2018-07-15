namespace AutoReviser
{
    using System;
    using System.Linq.Expressions;

    internal interface IReviser
    {
        bool TryRevise<T>(
            T instance, LambdaExpression predicate, out T revision);
    }
}
