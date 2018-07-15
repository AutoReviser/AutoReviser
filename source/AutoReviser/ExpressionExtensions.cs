namespace AutoReviser
{
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;

    internal static class ExpressionExtensions
    {
        public static object Evaluate(this Expression expression)
            => Lambda(expression).Compile().DynamicInvoke();
    }
}
