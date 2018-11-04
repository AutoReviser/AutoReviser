namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;
    using static System.Reflection.BindingFlags;

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
            var expression = predicate.Body as BinaryExpression;
            Expression left = expression.Left;
            Expression right = expression.Right;

            Queue<Expression> path = Deconstruct(left);

            switch (path.Dequeue())
            {
                case MethodCallExpression call:
                    if (path.Any())
                    {
                        ParameterExpression parameter = Parameter(call.Type);
                        Expression expr = parameter;
                        foreach (var e in path)
                        {
                            switch (e)
                            {
                                case MemberExpression m
                                when m.Member is PropertyInfo p:
                                    expr = MakeMemberAccess(expr, p);
                                    break;
                            }
                        }

                        BinaryExpression b = MakeBinary(ExpressionType.Equal, expr, right);
                        var lambda = Lambda(b, parameter);
                        var index = (int)call.Arguments[0].Evaluate();
                        var argument = Revise(call.Type, instance.Item(index), lambda);
                        return instance.SetItem(index, argument);
                    }
                    else
                    {
                        var index = (int)call.Arguments[0].Evaluate();
                        object item = right.Evaluate();
                        return instance.SetItem(index, item);
                    }

                default:
                    string message = "Could not parse the expression";
                    throw new NotSupportedException(message);
            }
        }

        private static Queue<Expression> Deconstruct(Expression left)
        {
            switch (left)
            {
                case MemberExpression member
                when member.Member is PropertyInfo property:
                    {
                        Queue<Expression> path = Deconstruct(member.Expression);
                        path.Enqueue(member);
                        return path;
                    }

                case MethodCallExpression call:
                    {
                        Queue<Expression> path = Deconstruct(call.Object);
                        path.Enqueue(call);
                        return path;
                    }

                default: return new Queue<Expression>();
            }
        }

        private static object Revise(
            Type typeArgument, object instance, LambdaExpression predicate)
        {
            // TODO: Cache the method instance.
            return typeof(ImmutableExtensions)
                .GetMethod("Revise", Public | Static)
                .MakeGenericMethod(typeArgument)
                .Invoke(default, new object[] { instance, predicate });
        }
    }
}
