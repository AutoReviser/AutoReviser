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
            ConstructorInfo[] constructors = typeof(T).GetConstructors(attr);
            return constructors.Single(c => IsCloneConstructor<T>(c) == false);
        }

        private static bool IsCloneConstructor<T>(ConstructorInfo constructor)
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            return parameters.Length == 1 && parameters[0].ParameterType == typeof(T);
        }
    }
}
