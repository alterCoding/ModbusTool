using System;
using System.Linq.Expressions;

namespace Modbus.Common.BCL
{
    internal static class CastHelper
    {
        private static LambdaExpression makeLambdaConvert(Type from , Type to, bool @checked)
        {
            var from_p = Expression.Parameter(from);
            Expression conv;

            if(from == to)
            {
                //optimization, nothing to be called ... is a noop
                conv = Expression.Block(to, from_p);
            }
            else
            {
                if (@checked) conv = Expression.ConvertChecked(from_p, to);
                else conv = Expression.Convert(from_p, to);
            }

            var cast_t = typeof(Func<,>).MakeGenericType(from, to);

            return Expression.Lambda(cast_t, conv, from_p);
        }

        public static class Checked<TFrom, TTo>
        {
            public static readonly Func<TFrom, TTo> _functor =
                makeLambdaConvert(typeof(TFrom), typeof(TTo), @checked: true).Compile() as Func<TFrom, TTo>;
        }
        public static class UnChecked<TFrom, TTo>
        {
            public static readonly Func<TFrom, TTo> _functor =
                makeLambdaConvert(typeof(TFrom), typeof(TTo), @checked: false).Compile() as Func<TFrom, TTo>;
        }
    }

    /// <summary>
    /// A static cast facility <br/>
    /// It enables us to workaround the stupid IConvertible interface. Because it's really painful to work with generics 
    /// and primitive types (IConvertible and Convert class are so limited ...)
    /// </summary>
    /// <typeparam name="TTo">the target type of the cast</typeparam>
    /// <remarks>This implementation uses linq compiled expression as the performance penalty is acceptable for most
    /// use cases (i.e they are better than a regular delegate)</remarks>
    public static class Cast<TTo>
    {
        public static TTo Checked<TFrom>(TFrom of)
        {
            return CastHelper.Checked<TFrom, TTo>._functor(of);
        }
        public static TTo UnChecked<TFrom>(TFrom of)
        {
            return CastHelper.UnChecked<TFrom, TTo>._functor(of);
        }
    }
}
