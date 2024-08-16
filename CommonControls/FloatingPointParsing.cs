using System;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// Helper for parsing of float32/64.<br/>
    /// The parsing result depends on the supplied <see cref="ValueFormatting"/> and more especially the formats which 
    /// have been registered
    /// </summary>
    /// <remarks>Cache instance for reuse (as instantiation is not cheap)</remarks>
    /// <typeparam name="TVal">float/double</typeparam>
    public class FloatingPointParsing<TVal> : IValueParsing<TVal>
        where TVal : unmanaged, IConvertible, IFormattable
    {
        public FloatingPointParsing(ValueFormatting formatting = null)
        {
            m_formatting = formatting ?? FormattedValue.Default;
        }

        public bool TryParse(string text, FormatOptions options, out FormattedValue<TVal> value)
        {
            ArithmeticValueFormat fmt = options.Format;

            if(tryParse(text, options, out TVal val))
            {
                value = FormattedValue.Create(val, options, m_formatting);
            }
            else
            { 
                if (string.IsNullOrWhiteSpace(text) && options.EmptyIsZero)
                    value = FormattedValue.Create(default(TVal), options, m_formatting);
                else
                    value = FormattedValue<TVal>.Null(options);
            }

            return value.IsEmpty == false;
        }

        public bool TryParse(string text, out FormattedValue<TVal> result)
        {
            return TryParse(text, FormatOptions.Default(ArithmeticValueFormat.@decimal), out result);
        }

        private bool tryParse(string text, FormatOptions options, out TVal value)
        {
            if (options != null && options.Format != ArithmeticValueFormat.@decimal)
                throw new InvalidOperationException($"Only Decimal format is expected, but was {options.Format}");

            return ParseGeneric.Default.TryParseFloatingPoint(text, out value);
        }

        private ValueFormatting m_formatting;

        //private static readonly TypeCode _typecode = Type.GetTypeCode(typeof(TVal));
    }
}
