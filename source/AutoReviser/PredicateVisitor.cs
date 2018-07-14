namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;
    using static System.Reflection.BindingFlags;

    internal struct PredicateVisitor
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

                default: Visit(expression.Left, expression.Right); break;
            }
        }

        private void Visit(Expression left, Expression right)
        {
            var path = new Stack<PropertyInfo>();
            Track(expression: left, path);
            UpdateArgument(path, right);
        }

        private void Track(Expression expression, Stack<PropertyInfo> path)
        {
            switch (expression)
            {
                case MemberExpression member
                when member.Member is PropertyInfo property:
                    path.Push(property);
                    Track(member.Expression, path);
                    break;
            }
        }

        private void UpdateArgument(
            Stack<PropertyInfo> path, Expression right)
        {
            PropertyInfo property = path.Pop();
            object value = path.Any()
                ? Evaluate(property, path, right)
                : Evaluate(right);
            _invoker.UpdateArgumentIfMatch(property, value);
        }

        private object Evaluate(
            PropertyInfo property,
            IEnumerable<PropertyInfo> path,
            Expression right)
        {
            Type propertyType = property.PropertyType;
            object instance = _invoker.FindArgument(property);
            LambdaExpression predicate =
                MakePredicateLambda(propertyType, path, right);
            return Revise(propertyType, instance, predicate);
        }

        private static LambdaExpression MakePredicateLambda(
            Type parameterType,
            IEnumerable<PropertyInfo> path,
            Expression right)
        {
            ParameterExpression parameter = Parameter(parameterType);
            Expression seed = parameter;
            Expression left = path.Aggregate(seed, MakeMemberAccess);
            BinaryExpression predicate = MakeEqualBinary(left, right);
            return Lambda(predicate, parameter);
        }

        private static BinaryExpression MakeEqualBinary(
            Expression left,
            Expression right)
        {
            return MakeBinary(ExpressionType.Equal, left, right);
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

        private static object Evaluate(Expression expression)
        {
            return Lambda(expression).Compile().DynamicInvoke();
        }
    }
}
