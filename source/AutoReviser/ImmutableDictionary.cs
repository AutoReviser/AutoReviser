namespace AutoReviser
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal static class ImmutableDictionary
    {
        public static bool IsImmutableDictionary<T>() => IsImmutableDictionary(typeof(T));

        private static bool IsImmutableDictionary(Type type)
        {
            const string name = "System.Collections.Immutable.ImmutableDictionary`2";
            return type.IsGenericType
                && type.GetGenericTypeDefinition().FullName == name;
        }

        public static object GetItem(object instance, object key)
        {
            return (instance as IDictionary)[key];
        }

        public static TImmutableDictionary SetItem<TImmutableDictionary>(
            TImmutableDictionary instance, object key, object value)
        {
            MethodInfo setItem = instance.GetType().GetMethod("SetItem");
            return (TImmutableDictionary)setItem.Invoke(instance, new[] { key, value });
        }
    }
}
