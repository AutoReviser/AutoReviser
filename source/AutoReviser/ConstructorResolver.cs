namespace AutoReviser
{
    using System.Linq;
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    internal static class ConstructorResolver
    {
        // TODO: Cache the constructor instance.
        public static ConstructorInfo Resolve<T>()
        {
            BindingFlags attr = Public | NonPublic | Instance;
            return typeof(T).GetConstructors(attr).Single();
        }
    }
}
