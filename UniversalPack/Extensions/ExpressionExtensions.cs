using System;
using System.Linq;
using System.Linq.Expressions;

namespace UniversalPack.Extensions
{
    public static class ExpressionExtensions
    {
        public static object GetValue(this Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                var objectMember = Expression.Convert(memberExpression, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();

                return getter();
            }

            if (expression is UnaryExpression unaryExpression)
            {
                var objectMember = Expression.Convert(unaryExpression, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();

                return getter();
            }

            var innerExpressionProp = expression
                .GetType()
                .GetProperties()
                .FirstOrDefault(e => e.Name.Equals("expression", StringComparison.InvariantCultureIgnoreCase));

            var constExpression = expression as ConstantExpression;

            if (constExpression == null && innerExpressionProp != null)
            {
                var innerExpression = innerExpressionProp.GetValue(expression) as Expression;

                return innerExpression?.GetValue();
            }

            if (constExpression != null)
            {
                return constExpression.Value;
            }

            var exception = new ApplicationException("Failed to gather argument value");
            var type = expression.GetType();

            exception.Data["Expression"] = expression;
            exception.Data["ExpressionToString"] = expression.ToString();
            exception.Data["Type"] = type;
            exception.Data["TypeFullName"] = type.FullName;

            throw exception;
        }
    }
}
