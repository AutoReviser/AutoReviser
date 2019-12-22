namespace AutoReviser
{
    using System;
    using System.Linq.Expressions;
    using static System.Reflection.BindingFlags;

    /// <summary>
    /// Provides extension methods to generate partially updated copies of immutable objects.
    /// </summary>
    public static class Reviser
    {
        private static readonly IReviser[] _revisers = new IReviser[]
        {
            new ImmutableArrayReviser(),
            new ImmutableDictionaryReviser(),
            new ObjectReviser(),
        };

        /// <summary>
        /// Create new object that is a partially updated copy of the specified source object.
        /// </summary>
        /// <typeparam name="T">The type of the immutable object.</typeparam>
        /// <param name="instance">The source object.</param>
        /// <param name="predicate">The lambda expression that describes what states the revised object should have.</param>
        /// <returns>A revised immutable object.</returns>
        /// <example>
        /// Here are immutable object classes.
        /// <code>
        /// public class ImmutableObject
        /// {
        ///     public ImmutableObject(int alfa, string bravo)
        ///         => (Alfa, Bravo) = (alfa, bravo);
        ///
        ///     public int Alfa { get; }
        ///
        ///     public string Bravo { get; }
        /// }
        ///
        /// public class ComplexImmutableObject
        /// {
        ///     public ComplexImmutableObject(int charile, ImmutableObject delta)
        ///         => (Charlie, Delta) = (charlie, delta);
        ///
        ///     public int Charlie { get; }
        ///
        ///     public ImmutableObject Delta { get; }
        /// }
        /// </code>
        /// A partially updated copy can be created by <c>Revise</c> method and a predicate expression.
        /// <code>
        /// var source = new ComplexImmutableObject(1, new ImmutableObject(2, "foo"));
        ///
        /// var revision = source.Revise(
        ///     x =>
        ///     x.Charlie == 10 &&
        ///     x.Delta.Bravo == "foo");
        /// </code>
        /// </example>
        public static T Revise<T>(
            this T instance, Expression<Func<T, bool>> predicate)
        {
            T revision = default;
            foreach (IReviser reviser in _revisers)
            {
                if (reviser.TryRevise(instance, predicate, out revision))
                {
                    break;
                }
            }

            return revision;
        }

        internal static object Revise(
            Type typeArgument, object instance, LambdaExpression predicate)
        {
            // TODO: Cache the method instance.
            return typeof(Reviser)
                .GetMethod("Revise", Public | Static)
                .MakeGenericMethod(typeArgument)
                .Invoke(default, new object[] { instance, predicate });
        }
    }
}
