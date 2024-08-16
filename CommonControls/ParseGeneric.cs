using System;
using System.Linq.Expressions;
using System.Globalization;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// A wrapper on {builtin-type}.TryParse() methods that enable use of generic.<br/>
    /// 
    /// </summary>
    /// <remarks>Must wait for .Core to get the BinarySpecifier</remarks>
    public class ParseGeneric
    {
        /// <summary>
        /// Process an invariant parsing
        /// </summary>
        public static readonly ParseGeneric Default = new ParseGeneric(invariant:true);
        /// <summary>
        /// Process parsing with current culture
        /// </summary>
        public static readonly ParseGeneric Current = new ParseGeneric(invariant:false);

        /// <summary>
        /// </summary>
        /// <param name="invariant">[TRUE] neutral number format shall be used (neutral culture)</param>
        public ParseGeneric(bool invariant)
        {
            m_invariant = invariant;
        }

        /// <summary>
        /// Any prefix isn't accepted due to underlying implementation limitations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryParseHexa<T>(string text, out T value)
        {
            var fun = m_invariant ? TryParseMethod<T>._invFunctor : TryParseMethod<T>._functor;
            return fun(text, NumberStyles.HexNumber, out value);
        }
        /// <summary>
        /// Accept only numbers w/o any exponent, w/ or w/o decimal point
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryParseDecimal<T>(string text, out T value)
        {
            var fun = m_invariant ? TryParseMethod<T>._invFunctor : TryParseMethod<T>._functor;
            return fun(text, NumberStyles.Number, out value);
        }
        public bool TryParseFloatingPoint<T>(string text, out T value)
        {
            var fun = m_invariant ? TryParseMethod<T>._invFunctor : TryParseMethod<T>._functor;
            return fun(text, NumberStyles.Float, out value);
        }
        public bool TryParseInteger<T>(string text, out T value)
        {
            var fun = m_invariant ? TryParseMethod<T>._invFunctor : TryParseMethod<T>._functor;
            return fun(text, NumberStyles.Integer, out value);
        }

        public bool TryParse<T>(string text, NumberStyles number, out T value)
        {
            var fun = m_invariant ? TryParseMethod<T>._invFunctor : TryParseMethod<T>._functor;
            return fun(text, number, out value);
        }

        private static LambdaExpression tryParseLambda(Type type, bool invariant)
        {
            var text_p = Expression.Parameter(typeof(string));
            var format_p = Expression.Parameter(typeof(NumberStyles));
            var provider_p = Expression.Constant(invariant ? NumberFormatInfo.InvariantInfo :NumberFormatInfo.CurrentInfo);
            var value_p = Expression.Parameter(type.MakeByRefType());

            var call = Expression.Call(type, "TryParse", null, text_p, format_p, provider_p, value_p);

            var signature_t = typeof(tryParseSignature<>).MakeGenericType(type);
            return Expression.Lambda(signature_t, call, text_p, format_p, value_p);
        }

        private delegate bool tryParseSignature<T>(string text, NumberStyles format, out T value);

        /// <summary>
        /// method cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class TryParseMethod<T>
        {
            public static readonly tryParseSignature<T> _invFunctor = 
                tryParseLambda(typeof(T), invariant:true).Compile() as tryParseSignature<T>;

            public static readonly tryParseSignature<T> _functor = 
                tryParseLambda(typeof(T), invariant:false).Compile() as tryParseSignature<T>;
        }

        private readonly bool m_invariant;
    }
}
