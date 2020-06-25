namespace AutoReviser
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.StringComparison;

    internal struct Argument
    {
        public Argument(Type parameterType, string parameterName, object value)
        {
            ParameterType = parameterType;
            ParameterName = parameterName;
            Value = CastIfNeeded(value, parameterType);
        }

        public Type ParameterType { get; }

        public string ParameterName { get; }

        public object Value { get; }

        public bool Match(PropertyInfo property)
            => property.PropertyType == ParameterType
            && property.Name.Equals(ParameterName, OrdinalIgnoreCase);

        public Argument SetValue(object value)
            => new Argument(ParameterType, ParameterName, value);

        private static object CastIfNeeded(object value, Type type)
        {
            if (value == null)
            {
                return value;
            }

            if (value.GetType() == type)
            {
                return value;
            }

            // TODO: Cache the lambda Delegate.
            ParameterExpression parameter = Expression.Parameter(value.GetType());
            UnaryExpression body = Expression.Convert(parameter, type);
            LambdaExpression lambda = Expression.Lambda(body, parameter);
            return lambda.Compile().DynamicInvoke(value);
        }
    }
}
