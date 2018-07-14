namespace AutoReviser
{
    using System.Collections.Generic;
    using System.Reflection;

    internal struct SeedVisitor
    {
        private readonly ConstructorInvoker _invoker;

        public SeedVisitor(ConstructorInvoker invoker) => _invoker = invoker;

        public void Visit(object seed)
        {
            PropertyInfo[] properties = seed.GetType().GetProperties();
            Visit(properties, instance: seed);
        }

        private void Visit(
            IEnumerable<PropertyInfo> properties, object instance)
        {
            foreach (PropertyInfo property in properties)
            {
                Visit(property, instance);
            }
        }

        private void Visit(PropertyInfo property, object instance)
        {
            object value = property.GetValue(instance);
            _invoker.UpdateArgumentIfMatch(property, value);
        }
    }
}
