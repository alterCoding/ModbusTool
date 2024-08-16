using System;

namespace Modbus.Common
{
    using BCL;

    /// <summary>
    /// Implementation base class for the <see cref="MBDataItem"/> specialization with an underlying type for the 
    /// value as such.<br/>
    /// The value and the associated text representation are locally cached, with the format that has been used to
    /// stringize the data
    /// </summary>
    /// <typeparam name="TVal">TValue: is the data type conveyed by the register(s)</typeparam>
    internal abstract class MBDataItem<TVal> : MBDataItemBase
        where TVal : unmanaged, IConvertible, IFormattable, IEquatable<TVal>
    {
        private MBDataItem(ushort addr, int index) : base(addr, index, default(TVal).GetTypeCode())
        {
        }
        protected MBDataItem(ushort addr, int index, FormattedValue<TVal> value): this(addr, index)
        {
            Value = value;
        }
        protected MBDataItem(ushort addr, int index, ArithmeticValueFormat fmt) : this(addr, index)
        {
            Value = FormattedValue<TVal>.Null(FormatOptions.Default(fmt));
        }
        protected MBDataItem(ushort addr, int index, FormatOptions fmt) : this(addr, index)
        {
            Value = FormattedValue<TVal>.Null(fmt);
        }

        public FormattedValue<TVal> Value 
        { 
            get => m_val; 

            protected set
            {
                if(!value.Equals(m_val))
                {
                    TVal old = m_val.Value;
                    m_val = value;

                    //only format may have been changed ... if so, we don't raise (but it could change in the future)
                    if (!old.Equals(value.Value))
                        raise(new ValueChangedEvent<TVal>(old, value.Value));
                }
            }
        }

        public override string Text => Value.Text;

        public override FormatOptions Formatting
        {
            get => Value.Format;

            set
            {
                //reformat
                if(value != Value.Format)
                    Value = FormattedValue.Create(Value.Value, value);
            }
        }

        /// <summary>
        /// Update value and (maybe) format, raising an event if value has been actually changed
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fmt"></param>
        /// <returns></returns>
        public string Update(TVal value, FormatOptions fmt)
        {
            Value = FormattedValue.Create(value, fmt);
            return Value.Text;
        }
        /// <summary>
        /// Update value, raising an event if value has been actually changed
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Update(TVal value)
        {
            Value = FormattedValue.Create(value, Value.Format);
            return Value.Text;
        }

        public override string ToString() => $"Addr:0x{Address:x04} index:{Index} val:{Value.Value}";

        private FormattedValue<TVal> m_val;
    }

    /// <summary>
    /// Is a <see cref="MBDataItem{TVal}"/> specialization, which stands for a regular 16bit modbus register. 
    /// </summary>
    /// <typeparam name="TVal">the builtin 16bit data type (i.e signed or unsigned short) but could be byte/sbyte too
    /// as a single byte can be mapped into the LSB of a native register</typeparam>
    internal class MBNativeDataItem<TVal> : MBDataItem<TVal> 
        where TVal : unmanaged, IConvertible, IFormattable, IEquatable<TVal>
    {
        public MBNativeDataItem(ushort addr, int index, FormattedValue<TVal> value) : base(addr, index, value) { }
        public MBNativeDataItem(ushort addr, int index, FormatOptions fmt) : base(addr, index, fmt) { }
        public MBNativeDataItem(ushort addr, int index, ArithmeticValueFormat fmt) : base(addr, index, fmt) { }

        public override bool TryParse(string text, FormatOptions options = null, IValueParsing parsing = null)
        {
            var u16parsing = (parsing as IValueParsing<TVal>) ?? _defaultParsing;

            bool success = u16parsing.TryParse(text, options ?? Formatting, out var value);

            if (success) Value = value;

            return !value.IsEmpty;
        }

        public override bool ReadFrom(IMBDataReader reader, Endianness _)
        {
            bool success = reader.TryGetRegister(Address, out ushort value);
            if (success) Update(Cast<TVal>.UnChecked(value));

            return success;
        }

        public override bool WriteTo(IMBDataWriter writer, Endianness _)
        {
            return writer.TrySetRegister(Address, Cast<ushort>.UnChecked(Value.Value));
        }

        private static readonly IValueParsing<TVal> _defaultParsing = new BytesValueParsing<TVal>(FormattedValue.Default);

        private static bool checkIs16bitOrLess()
        {
            if (PrimitiveTraits<TVal>.Size > 2)
                throw new NotSupportedException(
                    $"The underlying data type must be lesser or equal than 16bit. Got {typeof(TVal)}");

            return true;
        }

        private static readonly bool _staticCheck = checkIs16bitOrLess();
    }

    /// <summary>
    /// Is a <see cref="MBDataItem{TVal}"/> specialization, which stands for an extended 32bit register, by booking 2 
    /// contiguous native registers. The object could convey any kind of sizeof(4) types, i.e either an integer32 type
    /// or a float32 type
    /// </summary>
    internal class MBVirtual32DataItem<TVal> : MBDataItem<TVal>
        where TVal : unmanaged, IConvertible, IFormattable, IEquatable<TVal>
    {
        public MBVirtual32DataItem(ushort addr, int index, FormattedValue<TVal> value) : base(addr, index, value) { }
        public MBVirtual32DataItem(ushort addr, int index, FormatOptions fmt) : base(addr, index, fmt) { }
        public MBVirtual32DataItem(ushort addr, int index, ArithmeticValueFormat fmt) : base(addr, index, fmt) { }

        public override bool TryParse(string text, FormatOptions options = null, IValueParsing parsing = null)
        {
            var tparsing = (parsing as IValueParsing<TVal>) ?? _defaultParsing;

            bool success = tparsing.TryParse(text, options ?? Formatting, out var value);

            if (success) Value = value;

            return !value.IsEmpty;
        }

        public override bool WriteTo(IMBDataWriter writer, Endianness endianness)
        {
            uint dword;
            if(_typecode == TypeCode.Single)
            {
                //should be a noop, but as C#.generics are templates at a discount, we need to write stupid code
                float val = Cast<float>.UnChecked(Value.Value);

                //endianness should be noop, as we simply map native (float) to native (uint)
                Marshaller.ToBinary(val, out dword, Endianness.native);
            }
            else
            {
                dword = Cast<uint>.UnChecked(Value.Value);
            }

            return writer.TrySetRegisters(Address, dword, endianness);
        }

        public override bool ReadFrom(IMBDataReader reader, Endianness endianness)
        {
            if (!reader.TryGetRegisters(Address, out uint value, endianness)) return false;

            if(_typecode == TypeCode.Single)
            {
                float f32 = Marshaller.FloatFromBinary(value); //native endianness
                Update(Cast<TVal>.UnChecked(f32));
            }
            else
            {
                Update(Cast<TVal>.UnChecked(value));
            }

            return true;
        }

        private static bool checkIs32bit()
        {
            if (PrimitiveTraits<TVal>.Size != 4)
                throw new NotSupportedException($"The underlying data type must be 4-sizeof. Got {typeof(TVal)}");

            return true;
        }

        private static IValueParsing<TVal> initializeValueParsing()
        {
            if (typeof(TVal).IsFloatingPoint()) return new FloatingPointParsing<TVal>(FormattedValue.Default);
            else return new BytesValueParsing<TVal>(FormattedValue.Default);
        }

        private static readonly bool _staticCheck = checkIs32bit();

        private static readonly IValueParsing<TVal> _defaultParsing = initializeValueParsing();

        private static TypeCode _typecode = Type.GetTypeCode(typeof(TVal));
    }
}
