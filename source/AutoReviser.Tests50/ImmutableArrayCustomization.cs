namespace AutoReviser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Kernel;

    public class ImmutableArrayCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
            => fixture.Customizations.Add(new ImmutableArrayBuilder(fixture));

        private class ImmutableArrayBuilder : ISpecimenBuilder
        {
            private readonly IFixture _fixture;

            public ImmutableArrayBuilder(IFixture fixture) => _fixture = fixture;

            public object Create(object request, ISpecimenContext context)
            {
                switch (request)
                {
                    case Type type when IsImmutableArrayType(type):
                        return GenerateImmutableArrayInstance(type);

                    default: return new NoSpecimen();
                }
            }

            private bool IsImmutableArrayType(Type type)
                => type.IsValueType
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>);

            private object GenerateImmutableArrayInstance(Type type)
            {
                Type elementType = type.GenericTypeArguments[0];
                MethodInfo template = typeof(ImmutableArrayBuilder).GetMethod(
                    nameof(GenerateImmutableArray),
                    BindingFlags.NonPublic | BindingFlags.Instance);
                return template.MakeGenericMethod(elementType).Invoke(this, null);
            }

            private ImmutableArray<T> GenerateImmutableArray<T>()
            {
                IEnumerable<T> elements = _fixture.CreateMany<T>();
                return ImmutableArray.CreateRange(elements);
            }
        }
    }
}
