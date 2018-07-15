namespace AutoReviser
{
    using System;
    using static System.Reflection.BindingFlags;

    internal static class ImmutableCollectionExtensions
    {
        public static bool IsImmutableArray<T>(this T instance)
        {
            const string name = "System.Collections.Immutable.ImmutableArray`1";
            Type type = instance.GetType();
            return type.IsGenericType
                && type.GetGenericTypeDefinition().FullName == name;
        }

        public static T SetItem<T>(this T instance, int index, object item)
        {
            return (T)instance
                .GetType()
                .GetMethod("SetItem", Public | Instance)
                .Invoke(instance, new[] { index, item });
        }
    }
}
