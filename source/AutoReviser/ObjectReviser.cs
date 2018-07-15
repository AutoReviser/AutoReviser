namespace AutoReviser
{
    using System;
    using System.Linq.Expressions;

    internal class ObjectReviser : IReviser
    {
        public bool TryRevise<T>(
            T instance, LambdaExpression predicate, out T revision)
        {
            var invoker = ConstructorInvoker.Create<T>();
            new SeedVisitor(invoker).Visit(seed: instance);
            new PredicateVisitor(invoker, argument: instance).Visit(predicate);
            revision = (T)invoker.Invoke();
            return true;
        }
    }
}
