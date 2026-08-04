using System.Linq.Expressions;

namespace MHAuthorWebsite.Core.Common.Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        ParameterExpression param = Expression.Parameter(typeof(T));

        BinaryExpression body = Expression.AndAlso(
            Expression.Invoke(left, param),
            Expression.Invoke(right, param)
        );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}