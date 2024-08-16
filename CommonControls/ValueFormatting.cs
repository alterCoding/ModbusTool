using System;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// Value formatting attributes
    /// </summary>
    /// <remarks>class is readonly and has value type semantic</remarks>
    public class FormatOptions : IEquatable<FormatOptions>
    {
        public FormatOptions(ArithmeticValueFormat fmt, bool useAlt = false, int maxLen = 0, bool emptyIsZero = true)
        {
            Format = fmt;
            UseAlt = useAlt;
            MaxLength = maxLen;
            EmptyIsZero = emptyIsZero;
        }

        public bool Equals(FormatOptions o)
        {
            return o != null && Format == o.Format && UseAlt == o.UseAlt && MaxLength == o.MaxLength && EmptyIsZero == o.EmptyIsZero;
        }
        public override bool Equals(object obj) => obj is FormatOptions o && Equals(o);
        public override int GetHashCode() => (Format, UseAlt, MaxLength, EmptyIsZero).GetHashCode(); //poor impl
        public static bool operator ==(FormatOptions f1, FormatOptions f2)
        {
            return ReferenceEquals(f1, f2) || f1?.Equals(f2) == true;
        }
        public static bool operator !=(FormatOptions f1, FormatOptions f2) => !(f1 == f2);

        /// <summary>
        /// Formatting shall use the alternative (if any) format properties 
        /// </summary>
        public bool UseAlt { get; }

        public ArithmeticValueFormat Format { get; }

        /// <summary>
        /// Best effort directive to constrain the output size
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// An empty text input shall be resolved to a zero value
        /// </summary>
        public bool EmptyIsZero { get; }

        public static FormatOptions Default(ArithmeticValueFormat fmt)
        {
            if (fmt == ArithmeticValueFormat.hexa) return _hexa;
            else if (fmt == ArithmeticValueFormat.@decimal) return _decimal;
            else if (fmt == ArithmeticValueFormat.binary) return _binary;
            else return null;
        }

        private static readonly FormatOptions _binary = new FormatOptions(ArithmeticValueFormat.binary);
        private static readonly FormatOptions _decimal = new FormatOptions(ArithmeticValueFormat.@decimal);
        private static readonly FormatOptions _hexa = new FormatOptions(ArithmeticValueFormat.hexa);
    }

    /// <summary>
    /// Facade for value formatting operations, which are parameterized by internal <see cref="PrimitiveFormatter"/> 
    /// instances
    /// <para>Limited scope:<br/>
    /// Target types are USHORT (address and mbus registers), FLOAT32 for extended registers support. <br/>
    /// Other types haven't been partially tested, although they are implicitely exposed with the 'Format' generic 
    /// methods and the fallback formatting support
    /// </para>
    /// <para>Reusability notice:<br/>
    /// This class is neither generic nor totally reusable, as we have wrapped into this class some static rules for
    /// our applicative-scope value formatting needs (see the formatters composition)<br/>
    /// To summarize, further formatting customizations should be studied and implemented here
    /// </para>
    /// </summary>
    public class ValueFormatting
    {
        /// <summary>
        /// </summary>
        /// <param name="f32dig">The number of significative digits for the default output (expected to be lesser 
        /// than 9, which is the max round-trippable capability). It should be greater than <paramref name="f32digAlt"/>
        /// by consistency</param>
        /// <param name="f64dig">The number of significative digits for the default output (expected to be lesser
        /// lesser than 17, which is max round-trippable capability). It should be greater than <paramref name="f64digAlt"/>
        /// by consistency</param>
        /// <param name="f32digAlt">the number of significative digits for a more condensed output (expected to be
        /// lesser than 7, which is the default value for the float "G" format)</param>
        /// <param name="f64digAlt">the number of significative digits for a more condensed output (expected to be
        /// lesser than 15, which is the default value for the double "G" format)</param>
        public ValueFormatting(
            int f32dig = PrimitiveFormatter._floatStdPrecision,  //equ. to the standard precision (format 'G')
            int f64dig = PrimitiveFormatter._doubleStdPrecision, 
            int f32digAlt = 5, //a downward precision
            int f64digAlt = 12
            )
        {
            if (f32digAlt >= PrimitiveFormatter._floatStdPrecision) throw new ArgumentException(
                 $"expecting a float significative digits number lesser than the default precision ({PrimitiveFormatter._floatStdPrecision}) but was {f32digAlt}");

            if (f32dig > PrimitiveFormatter._floatRoundTripPrecision) throw new ArgumentException(
                 $"expecting a float significative digits number lesser than the max precision ({PrimitiveFormatter._floatRoundTripPrecision}) but was {f32dig}");

            if (f32digAlt > f32dig) throw new ArgumentException(
                 $"expecting a float significative digits number lesser than the default number ({f32dig}) but was {f32digAlt}");

            if (f64digAlt >= PrimitiveFormatter._doubleStdPrecision) throw new ArgumentException(
                 $"expecting a double significative digits number lesser than the default precision ({PrimitiveFormatter._doubleStdPrecision}) but was {f64digAlt}");

            if (f64dig > PrimitiveFormatter._doubleRoundTripPrecision) throw new ArgumentException(
                $"expecting a double significative digits number lesser than the max precision ({PrimitiveFormatter._doubleRoundTripPrecision}) but was {f64dig}");

            if (f64digAlt > f64dig) throw new ArgumentException(
                 $"expecting a double significative digits number lesser than the default number ({f64dig}) but was {f64digAlt}");

            //_f32dig = f32dig;
            //_f32digAlt = f32digAlt;
            //_f64dig = f64dig;
            //_f64digAlt = f64digAlt;

            _hexFormatter = new PrimitiveFormatter
            (
                PrimitiveFormatter.Format<ushort>.Hexa(upper: true, padding: 4),
                PrimitiveFormatter.Format<short>.Hexa(upper: true, padding: 4),
                PrimitiveFormatter.Format<int>.Hexa(upper: true, padding: 8),
                PrimitiveFormatter.Format<uint>.Hexa(upper: true, padding: 8),
                PrimitiveFormatter.Format<byte>.Hexa(upper: true, padding: 2),
                PrimitiveFormatter.Format<sbyte>.Hexa(upper: true, padding: 2),
                PrimitiveFormatter.Format<float>.FloatHexaPack()
            )
            .WithAlternative
            (
                PrimitiveFormatter.Format<ushort>.Hexa(upper: true, padding: 4, prefix: "", suffix:"\u2095"),
                PrimitiveFormatter.Format<float>.FloatHexaPack(prefix:string.Empty)
            );

            _decFormatter = new PrimitiveFormatter
            (
                PrimitiveFormatter.Format<float>.FloatingPoint(digits: 
                    f32dig == PrimitiveFormatter._floatStdPrecision ? 0 : f32dig),

                PrimitiveFormatter.Format<double>.FloatingPoint(digits: 
                    f64dig == PrimitiveFormatter._doubleStdPrecision ? 0 : f64dig)
            )
            .WithAlternative
            (
                PrimitiveFormatter.Format<float>.FloatingPoint(digits: 
                    f32digAlt == PrimitiveFormatter._floatStdPrecision ? 0 : f32digAlt, upper:false),

                PrimitiveFormatter.Format<double>.FloatingPoint(digits: 
                    f64digAlt == PrimitiveFormatter._doubleStdPrecision ? 0 : f64digAlt, upper:false)
            );

            _binFormatter = new PrimitiveFormatter
            (
                PrimitiveFormatter.Format<ushort>.Binary(padding: 16),
                PrimitiveFormatter.Format<short>.Binary(padding: 16),
                PrimitiveFormatter.Format<byte>.Binary(padding: 8),
                PrimitiveFormatter.Format<sbyte>.Binary(padding: 8)

                //for 32/64 bit, a special converter should be implementd because the trivial alignment of N bits is
                //totally indigest, i.e let's introduce a '_' words or bytes separator
            )
            .WithAlternative
            (
                //a more compact form (no padding, no prefix)
                PrimitiveFormatter.Format<ushort>.Binary(prefix: string.Empty),
                PrimitiveFormatter.Format<short>.Binary(prefix: string.Empty),
                PrimitiveFormatter.Format<byte>.Binary(prefix: string.Empty),
                PrimitiveFormatter.Format<sbyte>.Binary(prefix: string.Empty)
            );
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options">
        /// <see cref="FormatOptions.MaxLength"/> If the default formatting output is larger, the alternative format
        /// is used to get a more condensed output. Nevertheless, it might still be too large
        /// </param>
        /// <returns></returns>
        public string FormatInvariant<T>(T value, FormatOptions options) where T: unmanaged, IConvertible, IFormattable
        {
            var formatter = getFormatter(options.Format);

            string result = formatter.FormatValueInv(value, alt:options.UseAlt);

            if (options.UseAlt == false && options.MaxLength != 0 && result.Length > options.MaxLength)
            {
                //if max length is defined, we attempt to reformat with the alternative format
                result = formatter.FormatValueInv(value, alt: true);
            }

            return result;
        }

        public string FormatInvariant<T>(T value, ArithmeticValueFormat fmt) where T : unmanaged, IConvertible, IFormattable
        {
            return FormatInvariant(value, FormatOptions.Default(fmt)); 
        }

        /// <summary>
        /// Get the expected maximum length of the value formatting output
        /// </summary>
        /// <param name="fmt"></param>
        /// <returns>may be zero if not supported or irrelevant due to the data type and/or the format particularities</returns>
        public int GetMaxLengthOutput(TypeCode type, ArithmeticValueFormat fmt)
        {
            return GetMaxLengthOutput(type, FormatOptions.Default(fmt));
        }

        public int GetMaxLengthOutput(TypeCode type, FormatOptions options)
        {
            var formatter = getFormatter(options.Format);
            var format = formatter.GetFormatOrFallback(type, options.UseAlt);

            //if fallback does not hold the requested format, it's useless
            if (format.KindOfFormat != options.Format) return 0;
            else return format.MaxLength;
        }

        public string GetFormatPrefix(TypeCode type, FormatOptions options)
        {
            var formatter = getFormatter(options.Format);
            var format = formatter.GetFormatOrFallback(type, options.UseAlt);

            return format.FormatProperties.Prefix;
        }

        private PrimitiveFormatter getFormatter(ArithmeticValueFormat fmt)
        {
            return fmt == ArithmeticValueFormat.hexa ? _hexFormatter
                : fmt == ArithmeticValueFormat.@decimal ? _decFormatter
                : fmt == ArithmeticValueFormat.binary ? _binFormatter
                : null;
        }
       
        /// <summary>
        /// A default formatting task, overriden for USHORT and FLOAT32 <br/>
        /// - outputs "0xABCD" for the ushort ABCD <br/>
        /// - outputs "0xABCD1234" for a float (following the regular ieee754 encoding and representation) <br/>
        /// </summary>
        private readonly PrimitiveFormatter _hexFormatter;

        /// <summary>
        /// A default formatting task, overriden for USHORT, FLOAT32, FLOAT64 <br/>
        /// - outputs "1234" for the ushort 1234 <br/>
        /// - output float32/64 with "G" format, or (alternative) with a more condensed G6 
        /// </summary>
        private readonly PrimitiveFormatter _decFormatter;

        /// <summary>
        /// A default formatting task, overriden for USHORT <br/>
        /// - outputs "0b1000000000000001" for 0x8001
        /// - alternative shall be 1000000000000001 w/o the prefix
        /// </summary>
        private readonly PrimitiveFormatter _binFormatter;

        //private readonly int _f32dig, _f64dig;
        //private readonly int _f32digAlt, _f64digAlt;
    }
}
