namespace AutoReviser
{
    using System.Reflection;
    using AutoFixture;

    public class ImmutableDictionaryCustomizationAttribute : CustomizeAttribute
    {
        public override ICustomization GetCustomization(ParameterInfo parameter)
            => new ImmutableDictionaryCustomization();
    }
}
