using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CachedResourceManager
{
    public static class ReflectionExtensions
    {
        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var memberExpression = action.Body as MethodCallExpression;

            if (memberExpression != null)
            {
                return memberExpression.Method;
            }
            throw new ArgumentException(@"The member access expression does not access a method.", nameof(action));
        }
    }
}
