namespace AutoReviser
{
    using System.Linq;
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    internal static class ConstructorResolver
    {
        // TODO: Cache the constructor instance.
        public static ConstructorInfo Resolve<T>()
            => typeof(T).GetConstructors(Public | Instance).Single();
    }
}
