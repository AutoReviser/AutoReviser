namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;
    using static System.Reflection.BindingFlags;

    internal readonly struct PredicateVisitor
    {
        private readonly ConstructorInvoker _invoker;
        private readonly object _argument;

        public PredicateVisitor(ConstructorInvoker invoker, object argument)
            => (_invoker, _argument) = (invoker, argument);

        public void Visit(Expression expression)
        {
            switch (expression)
            {
                case LambdaExpression lambda: Visit(lambda); break;
                case BinaryExpression binary: Visit(binary); break;
            }
        }

        private void Visit(LambdaExpression expression)
            => Visit(expression.Body);

        private void Visit(BinaryExpression expression)
        {
            switch (expression.Left)
            {
                case BinaryExpression left
                when expression.Right is BinaryExpression right:
                    Visit(left);
                    Visit(right);
                    break;

                default:
                    Visit(expression.Left, expression.Right);
                    break;
            }
        }

        private void Visit(Expression left, Expression right)
        {
            Queue<Expression> path = Deconstruct(left);
            UpdateArgument(path, right);
        }

        private Queue<Expression> Deconstruct(Expression left)
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

                case UnaryExpression unary
                when unary.NodeType == ExpressionType.Convert:
                    {
                        return Deconstruct(unary.Operand);
                    }

                default: return new Queue<Expression>();
            }
        }

        private void UpdateArgument(
            Queue<Expression> path, Expression right)
        {
            switch (path.Dequeue())
            {
                case MemberExpression member
                when member.Member is PropertyInfo property:
                    if (path.Any())
                    {
                        Type propertyType = property.PropertyType;
                        ParameterExpression parameter = Parameter(propertyType);
                        Expression expr = parameter;
                        foreach (var e in path)
                        {
                            switch (e)
                            {
                                case MemberExpression m
                                when m.Member is PropertyInfo p:
                                    expr = MakeMemberAccess(expr, p);
                                    break;

                                case MethodCallExpression c:
                                    expr = Call(expr, c.Method, c.Arguments);
                                    break;
                            }
                        }

                        var predicate = MakeBinary(ExpressionType.Equal, expr, right);
                        var lambda = Lambda(predicate, parameter);
                        object instance = _invoker.FindArgument(property);
                        var argument = Revise(propertyType, instance, lambda);
                        _invoker.UpdateArgumentIfMatch(property, argument);

                        break;
                    }
                    else
                    {
                        object instance = _invoker.FindArgument(property);
                        var argument = Lambda(right).Compile().DynamicInvoke();
                        _invoker.UpdateArgumentIfMatch(property, argument);
                        break;
                    }
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
