namespace AutoReviser
{
    using System;
    using System.Reflection;
    using AutoFixture;

    public class ImmutableArrayCustomizationAttribute : CustomizeAttribute
    {
        public override ICustomization GetCustomization(ParameterInfo parameter)
            => new ImmutableArrayCustomization();
    }
}
