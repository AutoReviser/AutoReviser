namespace AutoReviser
{
    using System;
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;

    internal class ImmutableArrayReviser : IReviser
    {
        public bool TryRevise<T>(
            T instance, LambdaExpression predicate, out T revision)
        {
            if (instance.IsImmutableArray() == false)
            {
                revision = default;
                return false;
            }

            revision = Revise(instance, predicate);
            return true;
        }

        private static T Revise<T>(T instance, LambdaExpression predicate)
        {
            var lambda = predicate.Body as BinaryExpression;
            var method = lambda.Left as MethodCallExpression;
            var index = (int)method.Arguments[0].Evaluate();
            object item = lambda.Right.Evaluate();
            return instance.SetItem(index, item);
        }
    }
}
