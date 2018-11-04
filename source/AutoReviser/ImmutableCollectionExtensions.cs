namespace AutoReviser
{
    using System;
    using System.Collections;
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    internal static class ImmutableCollectionExtensions
    {
        public static bool IsImmutableArray<T>(this T instance)
            => instance.GetType().IsImmutableArray();

        private static bool IsImmutableArray(this Type type)
        {
            const string name = "System.Collections.Immutable.ImmutableArray`1";
            return type.IsGenericType
                && type.GetGenericTypeDefinition().FullName == name;
        }

        public static object Item(this object instance, int index)
            => ((IList)instance)[index];

        public static T SetItem<T>(this T instance, int index, object item)
        {
            return (T)instance
                .GetType()
                .GetMethod("SetItem", Public | Instance)
                .Invoke(instance, new[] { index, item });
        }

        public static bool IsImmutableArrayIndex(this MethodInfo method)
            => method.DeclaringType.IsImmutableArray()
            && method.Name == "get_Item";
    }
}
