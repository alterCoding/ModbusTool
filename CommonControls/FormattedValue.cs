using System;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// Entry point for formatting helper methods
    /// <para>
    /// Formatting is performed according to the supplied <see cref="ValueFormatting"/> instance or an internal default
    /// instance <br/>
    /// Formatting produces invariant outputs
    /// </para>
    /// </summary>
    public static class FormattedValue
    {
        public static FormattedValue<T> Create<T>(T value, FormatOptions options,  ValueFormatting formatting = null)
            where T : unmanaged, IConvertible, IFormattable
        {
            return new FormattedValue<T>(value, (formatting ?? Default).FormatInvariant(value, options), options);
        }
        public static FormattedValue<T> Create<T>(T value, ArithmeticValueFormat fmt, ValueFormatting formatting = null)
              where T : unmanaged, IConvertible, IFormattable
        {
            return new FormattedValue<T>(value, (formatting ?? Default).FormatInvariant(value, fmt), fmt);
        }
        public static FormattedValue<T> Zero<T>(ArithmeticValueFormat fmt, ValueFormatting formatting = null)
              where T : unmanaged, IConvertible, IFormattable
        {
            return new FormattedValue<T>(default(T), (formatting ?? Default).FormatInvariant(default(T), fmt), fmt);
        }
        public static FormattedValue<T> Zero<T>(FormatOptions fmt, ValueFormatting formatting = null)
              where T : unmanaged, IConvertible, IFormattable
        {
            return new FormattedValue<T>(default(T), (formatting ?? Default).FormatInvariant(default(T), fmt), fmt);
        }

        public static string Format<T>(T value, FormatOptions options,  ValueFormatting formatting = null)
            where T : unmanaged, IConvertible, IFormattable
        {
            return (formatting ?? Default).FormatInvariant(value, options);
        }
        public static string Format<T>(T value, ArithmeticValueFormat fmt,  ValueFormatting formatting = null)
            where T : unmanaged, IConvertible, IFormattable
        {
            return (formatting ?? Default).FormatInvariant(value, fmt);
        }

        public static readonly ValueFormatting Default = new ValueFormatting();
    }

    /// <summary>
    /// Cache a value with its text representation 
    /// </summary>
    /// <typeparam name="T">the value type</typeparam>
    public readonly struct FormattedValue<T> : IEquatable<FormattedValue<T>>
        where T : IConvertible, IFormattable 
    {
        internal FormattedValue(T value, string text, ArithmeticValueFormat fmt)
            : this(value, text, FormatOptions.Default(fmt))
        {
        }
        internal FormattedValue(T value, string text, FormatOptions fmt)
        {
            Value = value;
            Format = fmt;
            _text = text;
        }
        private FormattedValue(FormatOptions fmt)
        {
            Value = default;
            Format = fmt;
            _text = null;
        }

        public string Text => _text ?? string.Empty;

        public T Value { get; }

        public FormatOptions Format { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(_text);

        public bool Equals(FormattedValue<T> o) => Value.Equals(o.Value) && _text == o._text && Format == o.Format;
        public override bool Equals(object obj) => obj is FormattedValue<T> o && Value.Equals(o);
        public override int GetHashCode()
        {
            //is a poor implementation, but decent
            return (Value, _text, Format).GetHashCode();
        }

        public static bool operator ==(FormattedValue<T> v1, FormattedValue<T> v2) => v1.Equals(v2);
        public static bool operator !=(FormattedValue<T> v1, FormattedValue<T> v2) => v1.Equals(v2) == false;

        /// <summary>
        /// A special NULL value w/o any notion of format
        /// </summary>
        public static readonly FormattedValue<T> NULL = new FormattedValue<T>();

        /// <summary>
        /// A special NULL value with an expected format
        /// </summary>
        /// <param name="fmt"></param>
        /// <returns></returns>
        public static FormattedValue<T> Null(FormatOptions fmt) => new FormattedValue<T>(fmt);

        private readonly string _text;
    }
}
