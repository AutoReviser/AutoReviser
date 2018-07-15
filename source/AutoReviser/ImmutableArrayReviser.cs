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
            switch (instance)
            {
                case T _ when instance.IsImmutableArray():
                    Revise(instance, predicate, out revision);
                    return true;

                default:
                    revision = default;
                    return false;
            }
        }

        private static void Revise<T>(
            T instance, LambdaExpression predicate, out T revision)
        {
            var binary = predicate.Body as BinaryExpression;
            switch (binary.Left)
            {
                case MemberExpression member:
                    Expression right = binary.Right;
                    revision = SetItem(instance, member, right);
                    return;

                default:
                    revision = SetItem(instance, binary);
                    return;
            }
        }

        private static T SetItem<T>(
            T instance, MemberExpression memberExpression, Expression right)
        {
            var method = (MethodCallExpression)memberExpression.Expression;
            var index = (int)method.Arguments[0].Evaluate();
            Type elementType = instance.GetType().GetGenericArguments()[0];
            ParameterExpression parameter = Parameter(elementType);
            MemberExpression left = MakeMemberAccess(parameter, memberExpression.Member);
            BinaryExpression predicate = MakeBinary(ExpressionType.Equal, left, right);
            object item = instance.GetItem(index);
            LambdaExpression predicateLambda = Lambda(predicate, parameter);
            object revision = item.Revise(elementType, predicateLambda);
            return instance.SetItem(index, revision);
        }

        private static T SetItem<T>(T instance, BinaryExpression expression)
        {
            var method = (MethodCallExpression)expression.Left;
            var index = (int)method.Arguments[0].Evaluate();
            object item = expression.Right.Evaluate();
            return instance.SetItem(index, item);
        }
    }
}
