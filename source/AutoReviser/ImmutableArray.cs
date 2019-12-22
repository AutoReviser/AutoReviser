namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    internal static class ImmutableArray
    {
        public static bool IsImmutableArray<T>() => IsImmutableArray(typeof(T));

        private static bool IsImmutableArray(Type type)
        {
            const string name = "System.Collections.Immutable.ImmutableArray`1";
            return type.IsGenericType
                && type.GetGenericTypeDefinition().FullName == name;
        }

        public static object[] ToArray(object instance)
        {
            MethodInfo method = typeof(Enumerable)
                .GetMethod("ToArray")
                .MakeGenericMethod(instance.GetType().GetGenericArguments());

            return (object[])method.Invoke(obj: default, new[] { instance });
        }

        public static TImmutableArray Create<TImmutableArray>(object[] elements)
        {
            Type[] typeArguments = new[]
            {
                typeof(TImmutableArray),
                typeof(TImmutableArray).GetGenericArguments()[0],
            };

            return (TImmutableArray)typeof(ImmutableArray)
                .GetMethod(nameof(Factory), Static | NonPublic)
                .MakeGenericMethod(typeArguments)
                .Invoke(obj: default, new[] { elements });
        }

        private static object Factory<TImmutableArray, TElement>(
            object[] elements)
        {
            IEnumerable<TElement> items = elements.Cast<TElement>();
            object[] parameters = new[] { items };
            return typeof(TImmutableArray)
                .Assembly
                .GetType("System.Collections.Immutable.ImmutableArray")
                .GetMethods()
                .Where(m => m.Name == "CreateRange")
                .Where(m => m.GetParameters().Length == 1)
                .Single()
                .MakeGenericMethod(typeof(TElement))
                .Invoke(obj: default, parameters);
        }
    }
}
