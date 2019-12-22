namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using static ExpressionAssembler;
    using static System.Linq.Expressions.Expression;
    using static System.Reflection.BindingFlags;

    internal class ObjectReviser : IReviser
    {
        public bool TryRevise<T>(
            T instance, LambdaExpression predicate, out T revision)
        {
            var constructor = ConstructorResolver.Resolve<T>();
            var arguments = GetInitialArguments(constructor, instance);
            new PredicateVisitor(arguments).Visit(predicate);
            object[] parameters = arguments.Select(a => a.Value).ToArray();
            revision = (T)constructor.Invoke(parameters);
            return true;
        }

        private static Argument[] GetInitialArguments<T>(
            ConstructorInfo constructor, T instance)
        {
            Argument[] arguments = GetDefaultArguments(constructor);

            foreach (PropertyInfo property in GetProperties(instance))
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i].Match(property))
                    {
                        object value = property.GetValue(instance);
                        arguments[i] = arguments[i].SetValue(value);
                        break;
                    }
                }
            }

            return arguments;
        }

        private static Argument[] GetDefaultArguments(ConstructorInfo constructor)
        {
            IEnumerable<Argument> query =
                from p in constructor.GetParameters()
                let parameterType = p.ParameterType
                let name = p.Name
                let value = DefaultOf(p.ParameterType)
                select new Argument(parameterType, name, value);

            return query.ToArray();
        }

        private static object DefaultOf(Type type)
        {
            MethodInfo template = typeof(ObjectReviser)
                .GetMethod(nameof(Default), NonPublic | Static);
            MethodInfo method = template.MakeGenericMethod(type);
            return method.Invoke(obj: default, parameters: default);
        }

        private static T Default<T>() => default;

        private static PropertyInfo[] GetProperties<T>(T instance)
        {
            return instance
                .GetType()
                .GetProperties(Public | Instance | NonPublic);
        }

        private readonly struct PredicateVisitor
        {
            private readonly Argument[] _arguments;

            public PredicateVisitor(Argument[] arguments)
                => _arguments = arguments;

            public void Visit(LambdaExpression lambda)
                => Visit(lambda.Body);

            private void Visit(Expression expression)
            {
                switch (expression)
                {
                    case BinaryExpression binary: Visit(binary); break;
                }
            }

            private void Visit(BinaryExpression binary)
            {
                switch (binary.Left)
                {
                    case BinaryExpression left
                    when binary.Right is BinaryExpression right:
                        Visit(left);
                        Visit(right);
                        break;

                    default:
                        Visit(binary.Left, binary.Right);
                        break;
                }
            }

            private void Visit(Expression left, Expression right)
            {
                Queue<Expression> path = Disassemble(left);
                switch (path.Dequeue())
                {
                    case MemberExpression member
                    when member.Member is PropertyInfo property && path.Any():
                        ReviseArgument(property, reviser: value =>
                        {
                            Type propertyType = property.PropertyType;
                            LambdaExpression lambda = AssembleLambda(
                                Parameter(type: propertyType),
                                leftPath: path,
                                right);
                            return Reviser.Revise(propertyType, value, lambda);
                        });
                        break;

                    case MemberExpression member
                    when member.Member is PropertyInfo property:
                        ReviseArgument(property, _ => right.Evaluate());
                        break;
                }
            }

            private void ReviseArgument(
                PropertyInfo property, Func<object, object> reviser)
            {
                for (int i = 0; i < _arguments.Length; i++)
                {
                    if (_arguments[i].Match(property))
                    {
                        object value = _arguments[i].Value;
                        object newValue = reviser.Invoke(value);
                        _arguments[i] = _arguments[i].SetValue(newValue);
                        return;
                    }
                }

                string message = $"No constructor parameter was found to match the property '{property.Name}'.";
                throw new InvalidOperationException(message);
            }
        }
    }
}
