namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    internal struct ConstructorInvoker
    {
        private readonly ConstructorInfo _constructor;
        private readonly ConstructorArgument[] _arguments;

        private ConstructorInvoker(
            ConstructorInfo constructor, ConstructorArgument[] arguments)
        {
            (_constructor, _arguments) = (constructor, arguments);
        }

        public static ConstructorInvoker Create<T>()
            => Create(constructor: ConstructorResolver.Resolve<T>());

        private static ConstructorInvoker Create(ConstructorInfo constructor)
        {
            // TODO: Cache default argument array.
            IEnumerable<ConstructorArgument> argumentQuery =
                from p in constructor.GetParameters()
                let parameterType = p.ParameterType
                let name = p.Name
                let value = DefaultOf(p.ParameterType)
                select new ConstructorArgument(parameterType, name, value);

            ConstructorArgument[] arguments = argumentQuery.ToArray();

            return new ConstructorInvoker(constructor, arguments);
        }

        private static object DefaultOf(Type type)
        {
            // TODO: Cache the method instance.
            MethodInfo template = typeof(ConstructorInvoker)
                .GetMethod(nameof(Default), NonPublic | Static);
            MethodInfo method = template.MakeGenericMethod(type);
            return method.Invoke(obj: default, parameters: default);
        }

        private static T Default<T>() => default;

        public void UpdateArgumentIfMatch(
            PropertyInfo property, object value)
        {
            for (int i = 0; i < _arguments.Length; i++)
            {
                if (_arguments[i].Match(property))
                {
                    _arguments[i] = _arguments[i].ReviseWith(value);
                    break;
                }
            }
        }

        public object FindArgument(PropertyInfo property)
        {
            object argument = default;
            for (int i = 0; i < _arguments.Length; i++)
            {
                if (_arguments[i].Match(property))
                {
                    argument = _arguments[i].Value;
                    break;
                }
            }

            return argument;
        }

        public object Invoke()
        {
            object[] args = _arguments.Select(a => a.Value).ToArray();
            return _constructor.Invoke(args);
        }
    }
}
