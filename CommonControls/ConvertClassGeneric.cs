using System;
using System.Linq;
using System.Linq.Expressions;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// A wrapper over <see cref="System.Convert"/> class aimed to help us (a bit) when using generic types
    /// </summary>
    /// <remarks>
    /// <para>For now, not all methods set are wrapped, and it could be remained as is, as C# seems to make all his 
    /// best to discourage us to use generics with builtin types</para>
    /// <para>CACHE and reuse instance as create is not cheap (e.g using the Default instance and go ahead)</para>
    /// </remarks>
    public class ConvertClassGeneric
    {
        public static readonly ConvertClassGeneric Default = new ConvertClassGeneric();

        private ConvertClassGeneric() { }

        private static LambdaExpression toStringLambda(Type type)
        {
            if(!type.IsInteger())
                throw new InvalidCastException(
                    $"Type {type} cannot be stringize with a base conversion, please complain to .net guys ;-)");

            var value_p = Expression.Parameter(type);
            var base_p = Expression.Parameter(typeof(int));

            //the methods set Convert.ToString({type}, int base) is annoying since it does not exhibit a consistent api.
            //(int, short ...) are defined when the (uint, ushort) are not ... ok why not ... but byte is defined
            //and not sbyte ...
            //so what ?
            //If a type is not defined, we try and keep its (un)signed counterpart, adding a cast when required
            //
            //The resulting implementation is not really sexy, but dot.net doesn't help much when using (pseudo)
            //generics and primitive types. Nevertheless, compiled lambda are very effective

            LambdaExpression functor;
            try
            {
                var call = Expression.Call(typeof(Convert), nameof(Convert.ToString), null, value_p, base_p);
                functor = Expression.Lambda(call, value_p, base_p);
            }
            catch (InvalidOperationException)
            {
                //if method is not defined for the target type, its (un)signed variant should has been defined ----

                var cast = Expression.Convert(value_p, type.ToggleSigned());
                var call = Expression.Call(typeof(Convert), nameof(Convert.ToString), null, cast, base_p);
                functor = Expression.Lambda(call, value_p, base_p);
            }

            return functor;
        }


        private static LambdaExpression fromStringLambda(Type type)
        {
            if(!type.IsInteger())
                throw new InvalidCastException(
                    $"Type {type} cannot be parsed with a base conversion, please complain to .net guys ;-)");

            var string_p = Expression.Parameter(typeof(string));
            var base_p = Expression.Parameter(typeof(int));

            LambdaExpression functor;
            var call = Expression.Call(typeof(Convert), $"To{Type.GetTypeCode(type)}", null, string_p, base_p);
            functor = Expression.Lambda(call, string_p, base_p);

            return functor;
        }

        /// <summary>
        /// Perform a string conversion of an integer type according to the <paramref name="base"/> argument 
        /// (calling for example the <see cref="Convert.ToString(ushort, int)"/> method)
        /// </summary>
        /// <typeparam name="T">must be a true integer type since we inherit limits from the underlying implementation</typeparam>
        /// <param name="value"></param>
        /// <param name="base">basis for conversion result</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">if T cannot be stringize with a base conversion 
        /// (e.g a floating point)</exception>
        public string ToString<T>(T value, int @base) where T : IConvertible
        {
            return ToStringMethod<T>._functor(value, @base); 
        }

        /// <summary>
        /// Perform a parsing and conversion according to the <paramref name="base"/> argument (calling for example the
        /// <see cref="Convert.ToUInt16(string, int)"/> method
        /// </summary>
        /// <typeparam name="T">must be a true integer type since we inherit limits from the underlying implementation</typeparam>
        /// <param name="value"></param>
        /// <param name="base"></param>
        /// <returns></returns>
        /// <exception cref="Exception">exceptions from the underlying Convert class such as ArgumentException, 
        /// Overflow, OutOfRange, FormatException</exception>
        /// <exception cref="InvalidCastException">if T cannot be parsed by taking account a base conversion 
        /// (e.g a floating point)</exception>
        /// <remarks>Notice a few differences with the type.(Try)Parse methods. For example, ushort.TryParse(style.hex)
        /// is unable to parse 0xffff but ffff is ok, whereas Convert.ToUShort() returns the expected result. Some
        /// guys advise TryParse for both correctness and performances but I don't see that and could disagree. It
        /// migth also depend on framework vs. core and versions</remarks>
        public T FromString<T>(string value, int @base) where T : IConvertible
        {
            return FromStringMethod<T>._functor(value, @base);
        }

        /// <summary>
        /// Cache a compiled lambda expression that enables call to System.Convert.ToString(type, base) method(s). <br/>
        /// Only INTEGER types are expected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class ToStringMethod<T>
        {
            public static readonly Func<T, int, string> _functor =
                toStringLambda(typeof(T)).Compile() as Func<T, int, string>;
        }
        /// <summary>
        /// Cache a compiled lambda expression that enables call to System.Convert.To{type}(string, base) method(s). <br/>
        /// Only INTEGER types are expected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class FromStringMethod<T>
        {
            public static readonly Func<string, int, T> _functor =
                fromStringLambda(typeof(T)).Compile() as Func<string, int, T>;
        }
    }
}
