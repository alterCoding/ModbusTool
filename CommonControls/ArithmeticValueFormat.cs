using System;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// Define the data format representation of a number<br/>
    /// It does not mean anything about the encoded data type (int/float..., endianness...) 
    /// </summary>
    public enum ArithmeticValueFormat
    {
        @decimal,
        hexa,
        binary
    }

    /// <summary>
    /// Wrap an arithmetic underlying typeinfo and a number representation
    /// </summary>
    internal class ArithmeticValueType
    {
        public ArithmeticValueType(Type type)
            : this(type, type.IsFloatingPoint() ? ArithmeticValueFormat.@decimal : ArithmeticValueFormat.hexa)
        { 
        }
        public ArithmeticValueType(TypeCode type)
            : this(type.ToPrimitiveType())
        {
        }
        public ArithmeticValueType(Type type, ArithmeticValueFormat fmt)
        {
            if (type.IsArithmetic() == false)
                throw new NotSupportedException($"type {type} cannot be used with {nameof(ArithmeticValueType)}");

            //cache some underlying switches(...) although they are cheap. Are you happy, stupid early optimizer mind ?

            IsFloatingPoint = type.IsFloatingPoint();
            IsSigned = type.IsSigned();
            ValueCode = Type.GetTypeCode(type);

            Format = fmt;
            ValueType = type;
        }

        public ArithmeticValueFormat Format { get; }
        public Type ValueType { get; }
        public TypeCode ValueCode { get; }
        public bool IsFloatingPoint { get; protected set; }
        public bool IsSigned { get; protected set; }
        public bool IsInteger => !IsFloatingPoint;
    }

    /// <summary>
    /// Wrap an arithmetic underlying typeinfo and a number representation
    /// </summary>
    /// <typeparam name="TVal">must be an arithmetic builtin type</typeparam>
    internal class ArithmeticValueType<TVal> : ArithmeticValueType where TVal : unmanaged
    {
        public ArithmeticValueType() : base(typeof(TVal)) { }
        public ArithmeticValueType(ArithmeticValueFormat format) : base(typeof(TVal), format) { }
    }

}
