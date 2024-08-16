using System;

namespace Modbus.Common.BCL
{
    internal interface IValueParsing { }

    internal interface IValueParsing<TVal> : IValueParsing where TVal:IConvertible, IFormattable
    {
        bool TryParse(string text, FormatOptions options, out FormattedValue<TVal> value);
    }

    /// <summary>
    /// Helper for parsing of (U)64/32/16/8 integer values.<br/>
    /// The parsing result depends on the supplied <see cref="ValueFormatting"/> and more especially the formats which 
    /// have been registered
    /// </summary>
    /// <remarks>Cache instance for reuse (as instantiation is not cheap)</remarks>
    /// <typeparam name="TWord">
    /// Type of the expected single word : from signed/unsigned byte to signed/unsigned long
    /// </typeparam>
    internal class BytesValueParsing<TWord> : IValueParsing<TWord>
        where TWord : unmanaged, IConvertible, IFormattable
    {
        public BytesValueParsing(ValueFormatting formatting = null)
        {
            m_formatting = formatting ?? FormattedValue.Default;
        }

        /// <summary>
        /// Parse a general 16bit value from a text input following hexa or decimal formats
        /// </summary>
        /// <remarks>
        /// The parsed value is returned as unsigned type but signed values may be successfully parsed too 
        /// </remarks>
        /// <param name="text"></param>
        /// <param name="isHexDefault">when [TRUE] if the hexa prefix is not seen, the parsing attempts to match an
        /// hexadecimal input</param>
        /// <param name="value">the result. Shall be zero if parsing fails or input text was empty</param>
        public bool TryParse(string text, bool isHexDefault, out FormattedValue<TWord> value)
        {
            bool is_hex = stripFormatPrefix(text, ArithmeticValueFormat.hexa, out text);
            if (is_hex || isHexDefault)
            {
                TryParse(text, FormatOptions.Default(ArithmeticValueFormat.hexa), out value);
            }
            else
            {
                if (TryParse(text, FormatOptions.Default(ArithmeticValueFormat.@decimal), out value) == false) 
                {
                    //last chance, if valid hexa w/o prefix has been inputted, why the value should be rejected
                    TryParse(text, FormatOptions.Default(ArithmeticValueFormat.hexa), out value);
                }
            }

            return value.IsEmpty == false;
        }

  
        /// <summary>
        /// Parse a general 16bit value from a text input following the supplied format information
        /// </summary>
        /// <param name="text"></param>
        /// <param name="options">The expected format. Must not be null</param>
        /// <param name="value">The result if parsing was successful (or pseudo successful). <br/>
        /// Shall be <see cref="FormattedValue{T}.Null(FormatOptions)"/> if parsing has failed</param>
        /// <returns>Depending on <see cref="FormatOptions.EmptyIsZero"/> option, an empty input might be considered as 
        /// a valid input, thus <paramref name="value"/> shall be zero
        /// </returns>
        public bool TryParse(string text, FormatOptions options, out FormattedValue<TWord> value)
        {
            bool success;
            TWord val;
            ArithmeticValueFormat fmt = options.Format;

            if (fmt == ArithmeticValueFormat.hexa) success = tryParseHexa(text, options, out val);
            else if (fmt == ArithmeticValueFormat.@decimal) success = tryParseInteger(text, options, out val);
            else if (fmt == ArithmeticValueFormat.binary) success = tryParseBinary(text, options, out val);
            else { value = FormattedValue<TWord>.NULL; return false; }

            if (success)
                value = FormattedValue.Create(val, options, m_formatting);
            else
                value = FormattedValue<TWord>.Null(options);

            return value.IsEmpty == false;
        }

        private bool stripFormatPrefix(string text, ArithmeticValueFormat fmt, out string stripped, FormatOptions options = null)
        {
            if (options != null && options.Format != fmt)
                throw new InvalidOperationException($"{fmt} format is expected, but was {options.Format}");

            options = options ?? FormatOptions.Default(fmt);

            var prefix = m_formatting.GetFormatPrefix(_typecode, options);

            if (text.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                stripped = text.Substring(prefix.Length);
            else
                stripped = text;

            return stripped != text;
        }

        private bool tryParseHexa(string text, FormatOptions options, out TWord value)
        {
            stripFormatPrefix(text, ArithmeticValueFormat.hexa, out text, options);

            bool success = ParseGeneric.Default.TryParseHexa(text, out value);
            if(!success)
            {
                if(options.EmptyIsZero && string.IsNullOrWhiteSpace(text))
                {
                    success = true;
                    value = default;
                }
            }

            return success;
        }

        private bool tryParseInteger(string text, FormatOptions options, out TWord value)
        {
            if (options != null && options.Format != ArithmeticValueFormat.@decimal)
                throw new InvalidOperationException($"Decimal format is expected, but was {options.Format}");

            bool success = ParseGeneric.Default.TryParseInteger(text, out value);
            if(!success)
            {
                if(options.EmptyIsZero && 
                    (string.IsNullOrWhiteSpace(text) || (typeof(TWord).IsSigned() && text == "-")))
                {
                    success = true;
                    value = default;
                }
            }

            return success;
        }

        private bool tryParseBinary(string text, FormatOptions options, out TWord value)
        {
            stripFormatPrefix(text, ArithmeticValueFormat.binary, out text, options);

            try 
            {
                value = ConvertClassGeneric.Default.FromString<TWord>(text, 2);
                return true;
            }
            catch(Exception)
            {
                value = default(TWord);
                return false;
            }
        }

        private ValueFormatting m_formatting;

        private static readonly TypeCode _typecode = Type.GetTypeCode(typeof(TWord));
    }
}
