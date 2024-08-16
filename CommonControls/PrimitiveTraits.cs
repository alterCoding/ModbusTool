using System;
using System.Runtime.InteropServices;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// Implementation class for <see cref="PrimitiveTraits{T}"/>
    /// </summary>
    /// <remarks>C# generics are so poor ... that we need to write some pure old crappy code like that</remarks>
    internal static class NumericTraits
    {
        public static bool IsInteger(this Type type) => Type.GetTypeCode(type).IsInteger();
        public static bool IsInteger(this TypeCode type)
        {
            switch (type)
            {
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16: return true;
                default: return false;
            }
        }

        public static bool IsIntegral(this Type type) => Type.GetTypeCode(type).IsIntegral();
        public static bool IsIntegral(this TypeCode type)
        {
            if (type.IsInteger()) return true;
            else return type == TypeCode.Boolean || type == TypeCode.Char;
        }

        public static bool IsSigned(this Type type) => Type.GetTypeCode(type).IsSigned();
        public static bool IsSigned(this TypeCode type)
        {
            switch (type)
            {
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Decimal: return true;
                default: return false;
            }
        }

        public static bool IsUnsigned(this Type type) => Type.GetTypeCode(type).IsUnsigned();
        public static bool IsUnsigned(this TypeCode type)
        {
            switch (type)
            {
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Byte:
                case TypeCode.UInt16: return true;
                default: return false;
            }
        }
        public static bool IsFloatingPoint(this Type type) => Type.GetTypeCode(type).IsFloatingPoint();
        public static bool IsFloatingPoint(this TypeCode type)
        {
            return type == TypeCode.Single || type == TypeCode.Double || type == TypeCode.Decimal;
        }

        public static bool IsArithmetic(this Type type) => Type.GetTypeCode(type).IsArithmetic();
        public static bool IsArithmetic(this TypeCode type)
        {
            return type.IsIntegral() || type.IsFloatingPoint();
        }

        /// <summary>
        /// Make signed/unsigned a unsigned/signed type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>crappy code ... one more time ... C# generics are not C++ templates</remarks>
        public static Type ToggleSigned(this Type type)
        {
            if (type.IsInteger() == false)
                throw new InvalidOperationException($"it does not make sense to toggle sign of a non-integer type {type}");

            if(type.IsSigned())
            {
                if (type == typeof(sbyte)) return typeof(byte);
                else if (type == typeof(short)) return typeof(ushort);
                else if (type == typeof(int)) return typeof(uint);
                else if (type == typeof(long)) return typeof(ulong);
            }
            else
            {
                if (type == typeof(byte)) return typeof(sbyte);
                else if (type == typeof(ushort)) return typeof(short);
                else if (type == typeof(uint)) return typeof(int);
                else if (type == typeof(ulong)) return typeof(long);
            }

            //not reached
            return null;
        }

        /// <summary>
        /// Get the fundamental Type from a typecode
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <remarks>'string' is not considered as a builtin type. No debate, as TypeCode is a crappy abstraction</remarks>
        /// <exception cref="InvalidCastException">if <paramref name="code"/> is not related to a fundamental type</exception>
        public static Type ToPrimitiveType(this TypeCode code)
        {
            if (code == TypeCode.Object || code == TypeCode.Empty || code == TypeCode.String)
                throw new InvalidCastException($"TypeCode '{code.ToString()}' is not a builtin type");

            //a simple switch would be (certainly) more efficient. Feel free to profile ...
            return Type.GetType($"System.{code.ToString()}");
        }

        public static bool TryPrimitiveType(this TypeCode code, out Type type)
        {
            if (code == TypeCode.Object || code == TypeCode.Empty || code == TypeCode.String)
            {
                type = null;
                return false;
            }

            //a simple switch would be (certainly) more efficient. Feel free to profile ...
            type = Type.GetType($"System.{code.ToString()}");
            return true;
        }

        public static bool IsPrimitive(this TypeCode code) => code.TryPrimitiveType(out var _);
    }

    /// <summary>
    /// Fundamental types property traits
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Notice the possible controversy is_integer vs. is_integral. We favor pragmatism over theory correctness. As a
    /// result, char or bool are not considered as 'integer' (but only 'integral')
    /// </remarks>
    public static class PrimitiveTraits<T> where T:unmanaged
    {
        public static readonly bool IsInteger  = typeof(T).IsInteger();
        public static readonly bool IsIntegral = typeof(T).IsIntegral();
        public static readonly bool IsSigned = typeof(T).IsSigned();
        public static readonly bool IsUnsigned = typeof(T).IsUnsigned();
        public static readonly bool IsFloatingPoint = typeof(T).IsFloatingPoint();
        public static readonly bool IsArithmetic = typeof(T).IsArithmetic();
        public static readonly int Size = Marshal.SizeOf<T>();
    }
}
