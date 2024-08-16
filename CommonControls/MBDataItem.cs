using System;

namespace Modbus.Common
{
    using BCL;

    /// <summary>
    /// Event raised by <see cref="MBDataItem"/>
    /// </summary>
    internal class ValueChangedEvent { }

    /// <summary>
    /// Event raised by <see cref="MBDataItem{TVal}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ValueChangedEvent<T> : ValueChangedEvent
    {
        public ValueChangedEvent(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; }
        public T NewValue { get; }
    }

    internal delegate void ValueChangedHandler(MBDataItemBase sender, ValueChangedEvent change);

    /// <summary>
    /// A holder for a modbus address with the modbus register(s) data related to.<br/>
    /// It's a base class implementation w/o concrete data type
    /// </summary>
    internal abstract class MBDataItemBase
    {
        protected MBDataItemBase(ushort addr, int index, TypeCode type)
        {
            Address = addr;
            Index = index;
            m_valueType = new ArithmeticValueType(type); //default format(hexa or dec) depending on type
        }
 
        public ushort Address { get; }

        /// <summary>
        /// A local index for UI purpose
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Get type + format of the conveyed value
        /// </summary>
        public ArithmeticValueType ValueType
        {
            get
            {
                if(m_valueType.Format != Formatting.Format) //resync
                    m_valueType = new ArithmeticValueType(m_valueType.ValueType, Formatting.Format);

                return m_valueType;
            }
        }

        /// <summary>
        /// The holder also carries some attributes to the intent of the UI control from which the data is attached to
        /// </summary>
        public object UserParam { get; set; }

        public abstract string Text { get; }
        public abstract FormatOptions Formatting { get; set; }

        /// <summary>
        /// Attempt to update from a text value (referring to the current <see cref="Formatting"/> format if an actual
        /// new one is not provided with)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="options">Optional format options (otherwise the last <see cref="Formatting"/> elements shall 
        /// be used)</param>
        /// <param name="parsing">Parsing rules if default are not intended</param>
        /// <returns></returns>
        /// <remarks>an event shall be raised if value is changed</remarks>
        public abstract bool TryParse(string text, FormatOptions options = null, IValueParsing parsing = null);

        /// <summary>
        /// Write the underlying value to the supplied buffer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="endianness">the word endianness to be used (irrelevant if the underlying type is 16bit)</param>
        /// <returns></returns>
        public abstract bool WriteTo(IMBDataWriter writer, Endianness endianness);

        /// <summary>
        /// Read the underlying value from the supplied source 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="endianness">the source word endianness (irrelevant if the underlying type is 16bit)</param>
        /// <returns></returns>
        public abstract bool ReadFrom(IMBDataReader reader, Endianness endianness = Endianness.undefined);

        public event ValueChangedHandler OnValueChanged;

        protected void raise(ValueChangedEvent change) => OnValueChanged?.Invoke(this, change);

        private ArithmeticValueType m_valueType;
    }

    internal static class MBDataItem
    {
        /// <summary>
        /// Create a suitable concrete data item depending on the <typeparamref name="TVal"/> underlying value type.
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="addr"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="userParam">optional application parameters</param>
        /// <returns>
        /// - an instance of <see cref="MBVirtual32DataItem{TVal}"/> if the value type is sizeof(4) <br/>
        /// - an instance of <see cref="MBNativeDataItem{TVal}"/> when value type is sizeof(1/2)
        /// </returns>
        public static MBDataItem<TVal> Create<TVal>(ushort addr, int index, FormattedValue<TVal> value, 
            object userParam = null)
            where TVal : unmanaged, IConvertible, IFormattable, IEquatable<TVal>
        {
            if(PrimitiveTraits<TVal>.Size == 4)
                return new MBVirtual32DataItem<TVal>(addr, index, value) { UserParam = userParam };
            else
                return new MBNativeDataItem<TVal>(addr, index, value) { UserParam = userParam };
        }
    }
}
