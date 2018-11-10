namespace AutoReviser
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using static ExpressionAssembler;
    using static System.Linq.Expressions.Expression;

    internal class ImmutableArrayReviser : IReviser
    {
        public bool TryRevise<T>(
            T instance, LambdaExpression predicate, out T revision)
        {
            if (ImmutableArray.IsImmutableArray<T>() == false)
            {
                revision = default;
                return false;
            }
            else
            {
                revision = Revise(instance, predicate);
                return true;
            }
        }

        private static T Revise<T>(T instance, LambdaExpression predicate)
        {
            object[] elements = ImmutableArray.ToArray(instance);
            new PredicateVisitor(elements).Visit(lambda: predicate);
            return ImmutableArray.Create<T>(elements);
        }

        private readonly struct PredicateVisitor
        {
            private readonly object[] _elements;

            public PredicateVisitor(object[] elements) => _elements = elements;

            public void Visit(LambdaExpression lambda) => Visit(lambda.Body);

            private void Visit(Expression expression)
            {
                switch (expression)
                {
                    case BinaryExpression binary: Visit(binary); break;
                }
            }

            private void Visit(BinaryExpression binary)
            {
                switch (binary.Left)
                {
                    case BinaryExpression left
                    when binary.Right is BinaryExpression right:
                        Visit(left);
                        Visit(right);
                        break;

                    default:
                        Visit(binary.Left, binary.Right);
                        break;
                }
            }

            private void Visit(Expression left, Expression right)
            {
                Queue<Expression> path = Disassemble(left);
                var call = (MethodCallExpression)path.Dequeue();
                var index = (int)call.Arguments[0].Evaluate();
                if (path.Any())
                {
                    object element = _elements[index];
                    ParameterExpression parameter = Parameter(call.Type);
                    var lambda = AssembleLambda(parameter, path, right);
                    var newElement = Reviser.Revise(call.Type, element, lambda);
                    _elements[index] = newElement;
                }
                else
                {
                    _elements[index] = right.Evaluate();
                }
            }
        }
    }
}
