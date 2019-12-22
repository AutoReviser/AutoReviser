namespace AutoReviser
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;

    internal static class ExpressionAssembler
    {
        public static Queue<Expression> Disassemble(Expression expression) => expression switch
        {
            MemberExpression member when member.Member is PropertyInfo => Disassemble(member),
            MethodCallExpression call => Disassemble(call),
            UnaryExpression unary when unary.NodeType == ExpressionType.Convert => Disassemble(unary.Operand),
            _ => new Queue<Expression>(),
        };

        private static Queue<Expression> Disassemble(MemberExpression member)
        {
            Queue<Expression> path = Disassemble(member.Expression);
            path.Enqueue(member);
            return path;
        }

        private static Queue<Expression> Disassemble(MethodCallExpression call)
        {
            Queue<Expression> path = Disassemble(call.Object);
            path.Enqueue(call);
            return path;
        }

        public static LambdaExpression AssembleLambda(
            ParameterExpression parameter,
            Queue<Expression> leftPath,
            Expression right)
        {
            Expression left = parameter;
            foreach (Expression segment in leftPath)
            {
                left = Assemble(left, segment);
            }

            BinaryExpression predicate = MakeBinary(ExpressionType.Equal, left, right);
            return Lambda(predicate, parameter);
        }

        private static Expression Assemble(Expression left, Expression segment) => segment switch
        {
            MemberExpression member when member.Member is PropertyInfo property => MakeMemberAccess(left, property),
            MethodCallExpression call => Call(left, call.Method, call.Arguments),
            _ => left,
        };
    }
}
