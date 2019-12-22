namespace AutoReviser
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using static ExpressionAssembler;
    using static System.Linq.Expressions.Expression;

    internal class ImmutableDictionaryReviser : IReviser
    {
        public bool TryRevise<T>(T instance, LambdaExpression predicate, out T revision)
        {
            if (ImmutableDictionary.IsImmutableDictionary<T>() &&
                predicate.Body is BinaryExpression binary)
            {
                revision = Revise(instance, binary);
                return true;
            }
            else
            {
                revision = default;
                return false;
            }
        }

        private static T Revise<T>(T instance, BinaryExpression binary) => binary.Left switch
        {
            MethodCallExpression call => EvaluateBinaryWithCallLeft(instance, call, binary.Right),
            _ => EvaluateBinary(instance, binary.Left, binary.Right),
        };

        private static T EvaluateBinaryWithCallLeft<T>(
            T instance, MethodCallExpression left, Expression right)
        {
            object key = left.Arguments[0].Evaluate();
            object value = right.Evaluate();
            return ImmutableDictionary.SetItem(instance, key, value);
        }

        private static T EvaluateBinary<T>(
            T instance, Expression left, Expression right)
        {
            (MethodCallExpression call, LambdaExpression lambda) = Refactor(left, right);
            object key = call.Arguments[0].Evaluate();
            object oldValue = ImmutableDictionary.GetItem(instance, key);
            object newValue = Reviser.Revise(call.Type, oldValue, lambda);
            return ImmutableDictionary.SetItem(instance, key, newValue);
        }

        private static (MethodCallExpression call, LambdaExpression lambda) Refactor(
            Expression left, Expression right)
        {
            Queue<Expression> path = Disassemble(left);
            var call = (MethodCallExpression)path.Dequeue();
            ParameterExpression parameter = Parameter(call.Type);
            return (call, lambda: AssembleLambda(parameter, path, right));
        }
    }
}
