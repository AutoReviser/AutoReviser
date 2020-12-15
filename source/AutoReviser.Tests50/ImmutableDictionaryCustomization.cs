namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Kernel;

    public class ImmutableDictionaryCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
            => fixture.Customizations.Add(new ImmutableDictionaryBuilder(fixture));

        private class ImmutableDictionaryBuilder : ISpecimenBuilder
        {
            private readonly IFixture _fixture;

            public ImmutableDictionaryBuilder(IFixture fixture) => _fixture = fixture;

            public object Create(object request, ISpecimenContext context) => request switch
            {
                Type type when IsImmutableDictionaryType(type) => GenerateImmutableDictionaryInstance(type),
                _ => new NoSpecimen(),
            };

            private bool IsImmutableDictionaryType(Type type)
                => type.IsClass
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>);

            private object GenerateImmutableDictionaryInstance(Type type)
            {
                MethodInfo template = typeof(ImmutableDictionaryBuilder).GetMethod(
                    nameof(GenerateImmutableDictionary),
                    BindingFlags.NonPublic | BindingFlags.Instance);
                return template.MakeGenericMethod(type.GenericTypeArguments).Invoke(this, null);
            }

            private ImmutableDictionary<TKey, TValue> GenerateImmutableDictionary<TKey, TValue>()
            {
                return _fixture.Create<Dictionary<TKey, TValue>>().ToImmutableDictionary();
            }
        }
    }
}
