namespace AutoReviser
{
    using System;
    using System.Reflection;
    using static System.StringComparison;

    internal struct Argument
    {
        public Argument(Type parameterType, string parameterName, object value)
        {
            ParameterType = parameterType;
            ParameterName = parameterName;
            Value = value;
        }

        public Type ParameterType { get; }

        public string ParameterName { get; }

        public object Value { get; }

        public bool Match(PropertyInfo property)
            => property.PropertyType == ParameterType
            && property.Name.Equals(ParameterName, OrdinalIgnoreCase);

        public Argument SetValue(object value)
            => new Argument(ParameterType, ParameterName, value);
    }
}
