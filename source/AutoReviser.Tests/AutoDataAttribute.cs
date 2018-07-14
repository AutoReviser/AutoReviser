namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AutoDataAttribute : Attribute, ITestDataSource
    {
        private readonly Lazy<IFixture> _fixtureLazy;

        public AutoDataAttribute()
            : this(new Fixture())
        {
        }

        protected AutoDataAttribute(IFixture fixture)
            => _fixtureLazy = new Lazy<IFixture>(() => fixture);

        private IFixture Fixture => _fixtureLazy.Value;

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            yield return methodInfo.GetParameters().Select(Resolve).ToArray();
        }

        private object Resolve(ParameterInfo parameter)
        {
            foreach (IParameterCustomizationSource attribute in parameter
                .GetCustomAttributes()
                .OfType<IParameterCustomizationSource>())
            {
                attribute.GetCustomization(parameter).Customize(Fixture);
            }

            return new SpecimenContext(Fixture).Resolve(request: parameter);
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            IEnumerable<string> args = methodInfo
                .GetParameters()
                .Zip(data, (param, arg) => $"{param.Name}: {arg}");
            return $"{methodInfo.Name}({string.Join(", ", args)})";
        }
    }
}
