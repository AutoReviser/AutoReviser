namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class InternalExtensions
    {
        private static readonly Random _random = new Random();

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return from e in source
                   orderby _random.Next()
                   select e;
        }

        public static T Sample<T>(this IEnumerable<T> source)
        {
            return source.Shuffle().First();
        }
    }
}
