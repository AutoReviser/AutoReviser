namespace AutoReviser
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;

    internal static class ExpressionAssembler
    {
        public static Queue<Expression> Disassemble(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression member
                when member.Member is PropertyInfo:
                    {
                        Queue<Expression> path = Disassemble(member.Expression);
                        path.Enqueue(member);
                        return path;
                    }

                case MethodCallExpression call:
                    {
                        Queue<Expression> path = Disassemble(call.Object);
                        path.Enqueue(call);
                        return path;
                    }

                case UnaryExpression unary
                when unary.NodeType == ExpressionType.Convert:
                    {
                        return Disassemble(unary.Operand);
                    }

                default: return new Queue<Expression>();
            }
        }

        public static LambdaExpression AssembleLambda(
            ParameterExpression parameter,
            Queue<Expression> leftPath,
            Expression right)
        {
            Expression left = parameter;
            foreach (var segment in leftPath)
            {
                switch (segment)
                {
                    case MemberExpression member
                    when member.Member is PropertyInfo property:
                        left = MakeMemberAccess(left, property);
                        break;

                    case MethodCallExpression call:
                        left = Call(left, call.Method, call.Arguments);
                        break;
                }
            }

            var predicate = MakeBinary(ExpressionType.Equal, left, right);
            return Lambda(predicate, parameter);
        }
    }
}
