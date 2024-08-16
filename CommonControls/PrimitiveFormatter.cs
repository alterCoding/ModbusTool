using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// Formatting properties for format string, composite format string, or custom conversion functor 
    /// </summary>
    /// <remarks>A format does not have to support all attributes. Indeed most of them do not</remarks>
    internal class FormatAttributes
    {
        private FormatAttributes(string pattern)
        {
            if (pattern != null)
            {
                Pattern = (pattern ?? string.Empty).Trim();
                IsComposite = Regex.Match(Pattern, "{\\d+:.*?}").Success;
            }
        }
        private FormatAttributes(bool mutable)
        {
            Pattern = string.Empty;
            m_immutable = !mutable;
        }

        public static FormatAttributes MakeStringFormat(string pattern) => new FormatAttributes(pattern);
        public static FormatAttributes MakeEmpty() => new FormatAttributes(mutable:true);

        public static FormatAttributes Null = new FormatAttributes(mutable: false);

        /// <summary>
        /// The string format specifier. <br/>
        /// It may be null if the owner format uses a converter. It may be empty for a dummy format (which shall use an 
        /// empty object.ToString() call to format)
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// to be resolved with String.Format(..., args)
        /// </summary>
        public bool IsComposite { get; }

        public void Affixes(string prefix = "", string suffix = "")
        {
            ensureMutable();

            prefix = prefix?.Trim() ?? string.Empty;
            suffix = suffix?.Trim() ?? string.Empty;

            if (IsComposite)
            {
                //sanity check (partial) ---

                if (Pattern.StartsWith(prefix) == false)
                    throw new InvalidOperationException($"Invalid prefix '{prefix}' for format-string '{Pattern}'");
                if (Pattern.EndsWith(suffix) == false)
                    throw new InvalidOperationException($"Invalid suffix '{suffix}' for format-string '{Pattern}'");
            }

            Prefix = prefix;
            Suffix = suffix;
        }

        public void Padding(int count, char padWith = '0')
        {
            ensureMutable();
            m_padding = (count, padWith);
        }

        public string Prefix { get; private set; } = string.Empty;
        public string Suffix { get; private set; } = string.Empty;

        public int PadCount => m_padding.count;
        public char PadWith => m_padding.c;

        /// <summary>
        /// Flag for some formats that use case
        /// </summary>
        public bool Upper { get => m_upper; set { ensureMutable(); m_upper = value; } }


        private void ensureMutable()
        {
            if (m_immutable)
                throw new InvalidOperationException(
                    $"attempt to modify a readonly object {nameof(FormatAttributes)}");
        }

        private (int count, char c) m_padding;

        private bool m_upper;

        /** bullshit, but C# (as of 7.3) does not allow us to build *easily* good abstractions */
        private readonly bool m_immutable;
    }

    /// <summary>
    /// Facility to store relationships between a primitive type and a text format specification. Types are identified by
    /// their TypeCode. Format (see <see cref="Format.Spec"/> property) refers to an abstract specification, which can 
    /// be a simple format string to be used with <see cref="IFormattable.ToString(string, IFormatProvider)"/>  OR it
    /// can be a composite formatting string, to be used with <see cref="string.Format(IFormatProvider, string, object[])"/>
    /// </summary>
    public class PrimitiveFormatter
    {
        public const int _floatStdPrecision = 7;
        public const int _floatRoundTripPrecision = 9;
        public const int _doubleStdPrecision = 15;
        public const int _doubleRoundTripPrecision = 17;

        /// <summary>
        /// Formatting implementation detail
        /// </summary>
        /// <remarks>better to not expose</remarks>
        internal enum FormatSpecType
        {
            /// <summary>
            /// A string format is resolved with a call to {value}.ToString({format-spec})
            /// </summary>
            stringFormat,
            /// <summary>
            /// A composite format needs to be resolved with string.Format({format-spec}, {value ...}) 
            /// </summary>
            compositeFormat,
            /// <summary>
            /// A conversion format is resolved by calling {convert-functor}(value)
            /// </summary>
            conversion
        }

        /// <summary>
        /// Union-like that wraps a string pattern specifier or a conversion functor
        /// </summary>
        /// <typeparam name="T">the builtin type on which the format is applied to</typeparam>
        internal class FormatSpec<T> where T:IConvertible
        {
            public FormatSpec(string specifier)
            {
                FormatProperties = FormatAttributes.MakeStringFormat(specifier);

                if(FormatProperties.IsComposite) SpecType = FormatSpecType.compositeFormat;
                else SpecType = FormatSpecType.stringFormat;
            }

            public FormatSpec(Func<T, string> converter)
            {
                FormatProperties = FormatAttributes.MakeEmpty();

                Converter = converter;
                SpecType = FormatSpecType.conversion;
            }

            public FormatSpecType SpecType { get; }

            public FormatAttributes FormatProperties { get; private set; }

            /// <summary>
            /// The format specifier to be used. <br/>
            /// - It may be a (to)string format or a composite string.Format <br/>
            /// - It might be null if the value needs to be formatted by using a conversion functor 
            /// </summary>
            public string FormatString => FormatProperties.Pattern;

            /// <summary>
            /// The conversion functor to be called if (and only if) the value needs to be formatted by using a 
            /// convrter <br/>
            /// It's null if a string pattern must be used
            /// </summary>
            public Func<T, string> Converter { get; }
        }

        /// <summary>
        /// Generic format specification
        /// </summary>
        /// <remarks>
        /// <para>About buffer sizing:</para>
        /// https://stackoverflow.com/questions/1701055/what-is-the-maximum-length-in-chars-needed-to-represent-any-double-value
        /// Here we've DIY a bit, to keep an easier and sketchy solution
        /// </remarks>
        public abstract class Format
        {
            public TypeCode Type { get; }

            /// <summary>
            /// User format is a true custom format (i.e it hasn't been created using the factory methods set from 
            /// <see cref="Format{T}"/>)
            /// </summary>
            public bool IsUserFormat { get; protected set; }

            /// <summary>
            /// TRUE if this format has not been explicitely defined. It's a useless format as it doesn't wrap any
            /// formatting rule/specifier ... it leads to an object.ToString() call
            /// </summary>
            public bool IsFallback { get; private set; }

            /// <summary>
            /// A composite format needs to be resolved with string.Format({format-spec}, {value ...}) 
            /// </summary>
            public abstract bool IsComposite { get; }
 
            /// <summary>
            /// The formatting needs a converter function call
            /// </summary>
            public abstract bool NeedsConverter { get; }

            /// <summary>
            /// The expected max length of a formatted value. 0 means undefined (no limit or irrelevant or don't want
            /// to constrain)<br/>
            /// If not provided with, a default value is set according to the underlying data type and the associated
            /// format (but only if the <see cref="Format"/> is constructed with static factories methods)
            /// </summary>
            public int MaxLength { get; protected set; }

            public ArithmeticValueFormat KindOfFormat { get; }

            /// <summary>
            /// Get the format string specifier if the format has been defined it (or Empty if the format use a 
            /// conversion functor)
            /// </summary>
            /// <returns></returns>
            internal string GetFormatStringOrDefault() => FormatProperties.Pattern ?? string.Empty;

            internal abstract FormatAttributes FormatProperties { get; }

            /// <summary>
            /// Construct a fallback format which does not offer much than a ToString() call. Fallback objects are
            /// used to avoid nasty null instances and to enable a few commodities (as a maxLength decent approximation)
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            internal static Format MakeFallback(TypeCode type)
            {
                Format fmt;
                //yes it's awful, but C# generics don't help us at all
                switch(type)
                {
                    case TypeCode.Byte: fmt = Format<byte>.Integer(); break;
                    case TypeCode.SByte: fmt = Format<sbyte>.Integer(); break;
                    case TypeCode.UInt16: fmt = Format<ushort>.Integer(); break;
                    case TypeCode.Int16: fmt = Format<short>.Integer(); break;
                    case TypeCode.UInt32: fmt = Format<uint>.Integer(); break;
                    case TypeCode.Int32: fmt = Format<int>.Integer(); break;
                    case TypeCode.UInt64: fmt = Format<ulong>.Integer(); break;
                    case TypeCode.Int64: fmt = Format<long>.Integer(); break;
                    case TypeCode.Single: fmt = Format<float>.FloatingPoint(); break;;
                    case TypeCode.Double: fmt = Format<double>.FloatingPoint(); break;
                    default: throw new NotImplementedException($"no format fallback for {type}");
                };

                fmt.IsFallback = true;
                return fmt;
            }

            protected Format(TypeCode type, ArithmeticValueFormat kindOf)
            {
                Type = type;
                KindOfFormat = kindOf;
            }
        }

        /// <summary>
        /// Croncrete format specification
        /// </summary>
        /// <typeparam name="T">the builtin type on which the formatting applies</typeparam>
        public class Format<T> : Format where T : unmanaged, IConvertible
        {
            /// <summary>
            /// </summary>
            /// <param name="spec">the formatting detail</param>
            /// <param name="kindOf"></param>
            internal Format(FormatSpec<T> spec, ArithmeticValueFormat kindOf)
                : base(default(T).GetTypeCode(), kindOf)
            {
                Spec = spec;
            }

            /// <summary>
            /// Makes an explicit format
            /// </summary>
            /// <param name="spec">a tostring format or a composite string format</param>
            /// <returns></returns>
            public static Format<T> User(string spec, ArithmeticValueFormat kindOf)
            {
                return new Format<T>(new FormatSpec<T>(spec), kindOf)
                {
                    IsUserFormat = true,
                };
            }

            /// <summary>
            /// Makes a composite string, aimed to render "0x[padding:=0n]{value-hHex}", such as "0x000A" or "0xa", 
            /// calling string.Format(spec, 10)
            /// </summary>
            /// <param name="upper"></param>
            /// <param name="padding"></param>
            /// <typeparam name="T">the value type, typically USHORT in our app domain</typeparam>
            /// <returns></returns>
            public static Format<T> Hexa(int padding = 0, bool upper = false, string prefix = "0x", string suffix = "")
            {
                var spec = new FormatSpec<T>
                (
                    $"{prefix}{{0:{(upper ? "X" : "x")}" +
                    $"{(padding == 0 ? string.Empty : string.Concat("0", padding.ToString()))}}}{suffix}"
                );
                spec.FormatProperties.Padding(padding);
                spec.FormatProperties.Upper = upper;
                spec.FormatProperties.Affixes(prefix, suffix);

                return new Format<T>(spec, ArithmeticValueFormat.hexa)
                {
                    MaxLength = Marshal.SizeOf<T>() * 2 + prefix.Length + suffix.Length
                };
            }

            /// <summary>
            /// Make a format string w/o pre/post-fixes, aimed to simply render "[padding:=0n]{value-Hhex}", 
            /// such as "000A" or "a", calling 10.ToString(spec)
            /// </summary>
            /// <param name="upper"></param>
            /// <param name="padding"></param>
            /// <returns></returns>
            public static Format<T> ShortHexa(bool upper = false, int padding = 0)
            {
                var spec = new FormatSpec<T>
                (
                    $"{(upper ? "X" : "x")}{(padding == 0 ? string.Empty : string.Concat("0", padding.ToString()))}"
                );
                spec.FormatProperties.Upper = upper;
                spec.FormatProperties.Padding(padding);

                return new Format<T>(spec, ArithmeticValueFormat.hexa)
                { 
                    MaxLength = Marshal.SizeOf<T>() * 2
                };
            }

            /// <summary>
            /// Make a composite format string, aimed to render a value type from the concatenation of <paramref name="n"/> 
            /// values, which should be sub parts of the enclosing value type<br/>
            /// <para>Example:<br/>
            /// - Use HexaN() to render e.g a float32 value, which is outputted as '0xABCD0001' according to the pattern 
            /// "0x{[padding:=0n]{part0-hHex}}...{[padding:=0n]{partN-hHex}}" <br/>
            /// - using the call string.Format(spec, word0, word1)
            /// </para>
            /// </summary>
            /// <param name="n">should be the number of arguments for the string.Format(spec, [args]) call</param>
            /// <param name="upper"></param>
            /// <param name="padding"></param>
            /// <returns></returns>
            public static Format<T> HexaN(int n, bool upper = false, int padding = 0, string prefix = "0x")
            {
                var spec = new FormatSpec<T>
                (
                    string.Concat(Enumerable.Range(0, n).Select(i => formatPart(i)).Prepend(prefix))
                );
                spec.FormatProperties.Padding(padding);
                spec.FormatProperties.Upper = upper;
                spec.FormatProperties.Affixes(prefix);

                return new Format<T>(spec, ArithmeticValueFormat.hexa)
                { 
                    MaxLength = n * Marshal.SizeOf<T>() + prefix.Length
                };

                string formatPart(int i)
                {
                    return $"{{{i}:{(upper ? "X" : "x")}{(padding == 0 ? string.Empty : string.Concat("0", padding.ToString()))}}}";
                }
            }

            /// <summary>
            /// Makes a simple format string, using the default (D)ecimal format
            /// </summary>
            /// <param name="padding"></param>
            /// <returns></returns>
            public static Format<T> Integer(int padding = 0)
            {
                //crappy work ... doesn't it ? but C# generics are so poor --------------
                T type = default;
                int max_len = 0;
                int size = Marshal.SizeOf<T>();
                if (size == 1) max_len = type is byte ? 3 : 4; //255 -128
                else if (size == 2) max_len = type is ushort ? 5 : 6; //65,536 -32768
                else if (size == 4) max_len = type is uint ? 10 : 11; //‭4,294,967,295‬ -2,147,483,648‬
                else if (size == 8) max_len = 20; //‭18,446,744,073,709,551,615, -9,223,372,036,854,775,808‬
                else throw new NotImplementedException();

                var spec = new FormatSpec<T>($"D{(padding == 0 ? string.Empty : padding.ToString())}");
                spec.FormatProperties.Padding(padding);

                return new Format<T>(spec, ArithmeticValueFormat.@decimal)
                { 
                    MaxLength = max_len
                };
            }

            /// <summary>
            /// Makes a simple format string, using the default (G)eneral format
            /// </summary>
            /// <param name="digits">number of significant digits. 0 means using default significant digits, depending on
            /// the target type</param>
            /// <returns></returns>
            public static Format<T> FloatingPoint(int digits = 0, bool upper = true)
            {
                //really crappy work ......
                T type = default;
                int max_len = 0;
                if (type is float)
                {
                    max_len = digits == 0 ? _floatStdPrecision : digits; //G(float) default precision is 7, but round-trippable is 9
                    max_len += 6; //sign + decimal + (eE)xponent(+dd)
                }
                else if (type is double)
                {
                    max_len = digits == 0 ? _doubleStdPrecision : digits; //6(double) default precision is 15, but round trippable is 17
                    max_len += 7; //sign + decimal + (eE)xponent(+ddd)
                }
                else
                    throw new NotImplementedException();

                var spec = new FormatSpec<T>
                (
                    $"{(upper ? 'G' : 'g')}{(digits == 0 ? string.Empty : digits.ToString())}"
                );
                spec.FormatProperties.Padding(digits);
                spec.FormatProperties.Upper = upper;

                return new Format<T>(spec, ArithmeticValueFormat.@decimal)
                {
                    MaxLength = max_len,
                };
            }

            public static Format<T> Binary(int padding = 0, string prefix = "0b")
            {
                int @sizeof = Marshal.SizeOf<T>();
                int max_len = @sizeof * 8;

                var spec = new FormatSpec<T>
                (
                    t => string.Concat(prefix, ConvertClassGeneric.Default.ToString(t, 2).PadLeft(max_len, '0'))
                );
                spec.FormatProperties.Padding(padding);
                spec.FormatProperties.Affixes(prefix);

                return new Format<T>(spec, ArithmeticValueFormat.binary)
                {
                    MaxLength = prefix.Length + max_len
                };
            }

            public static Format<T> FloatHexaPack(bool upper = true, string prefix = "0x")
            {
                FormatSpec<T> spec = null;

                if(typeof(T) == typeof(float))
                {
                    spec = new FormatSpec<T>
                    (
                        t =>
                        {
                            float fval = t.ToSingle(null); //noop 
                            Marshaller.ToBinary(fval, out uint dw);
                            return string.Concat(prefix, dw.ToString($"{(upper ? "X8" : "x8")}"));
                        }
                    );
                }
                else if(typeof(T) == typeof(double))
                {
                    throw new NotSupportedException($"Don't know how to pack in hexa the floating point type {typeof(T)}");
                }
                else
                {
                    throw new NotSupportedException($"Don't know how to pack in hexa the floating point type {typeof(T)}");
                }

                int max_len = 2 * Marshal.SizeOf<T>();
                spec.FormatProperties.Padding(max_len);
                spec.FormatProperties.Upper = upper;
                spec.FormatProperties.Affixes(prefix);
             
                return new Format<T>(spec, ArithmeticValueFormat.hexa)
                {
                    MaxLength = max_len + prefix.Length
                };
            }

            /// <summary>
            /// The formatting specification detail
            /// </summary>
            internal FormatSpec<T> Spec { get; }

            public override bool IsComposite => Spec.SpecType == FormatSpecType.compositeFormat;

            public override bool NeedsConverter => Spec.SpecType == FormatSpecType.conversion;

            internal override FormatAttributes FormatProperties => Spec.FormatProperties;
        }

        public PrimitiveFormatter(params Format[] formats)
        {
            foreach (var fmt in formats) _formats[(int)fmt.Type] = fmt;
        }

        public PrimitiveFormatter WithAlternative(params Format[] formats)
        {
            foreach (var fmt in formats) _altFormats[(int)fmt.Type] = fmt;

            return this;
        }

        /// <summary>
        /// Get the settled format (if any) related to a value type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="alt">alternative format wanted (if any)</param>
        /// <returns>
        /// - NULL if undefined (by requesting the alternative format, if it's undefined whereas a main format has
        /// been defined, the latter is returned, and conversely) <br/>
        /// - NULL means please use a default ToString() or everything else, but nothing special has been defined here
        /// </returns>
        internal Format GetFormat(TypeCode type, bool alt = false)
        {
            int t = (int)type;
            return alt ? _altFormats[t] ?? _formats[t] : _formats[t] ?? _altFormats[t];
        }

        /// <summary>
        /// Get the settled format (if any) related to a value type. If any format has not been defined for the supplied
        /// type, returns a fallback format object, which holds a default format string specifier, for a general decimal
        /// output. Such a format is a bit useless, but it helps to mitigate the null reference risk for the caller
        /// </summary>
        /// <param name="type"></param>
        /// <param name="alt"></param>
        /// <returns>not null</returns>
        public Format GetFormatOrFallback(TypeCode type, bool alt = false)
        {
            var fmt = GetFormat(type, alt);
            if (fmt != null) return fmt;

            var fallback = _fallbacks[(int)type];
            if(fallback == null)
            {
                fallback = Format.MakeFallback(type);
                _fallbacks[(int)type] = fallback;
            }

            return fallback;
        }
             
        /// <summary>
        /// Get the settled format object (if any) related to the supplied <typeparamref name="T"/> type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="alt"></param>
        /// <returns>not null</returns>
        public Format<T> GetFormatOrFallback<T>(bool alt = false) where T : unmanaged, IConvertible
        {
            T type = default;
            //cast should be safe by design
            return GetFormatOrFallback(type.GetTypeCode(), alt) as Format<T>;
        }

        /// <summary>
        /// Get the settled format (if any) related to a value type
        /// </summary>
        /// <param name="alt">alternative format wanted (if any)</param>
        /// <returns>see <see cref="GetFormat(TypeCode, bool)"/>
        /// </returns>
        internal Format<T> GetFormat<T>(bool alt = false) where T : unmanaged, IConvertible
        {
            //cast should be safe by design
            return GetFormat(Type.GetTypeCode(typeof(T)), alt) as Format<T>;
        }

        internal bool HaveFormat<T>(bool alt = false) where T : unmanaged, IConvertible
        {
            return GetFormat(Type.GetTypeCode(typeof(T)), alt) != null;
        }

        #region formatting facade

        /// <summary>
        /// Format value with current culture
        /// </summary>
        /// <typeparam name="T">the builtin type to be formatted</typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public string FormatValue<T>(T value, bool alt = false) where T : unmanaged, IConvertible, IFormattable
        {
            var format = GetFormat<T>(alt);

            if (format == null)
            {
                return value.ToString();
            }
            else
            {
                if (format.IsComposite) return string.Format(format.GetFormatStringOrDefault(), value);
                else if (format.NeedsConverter) return format.Spec.Converter(value);
                else return value.ToString(format.GetFormatStringOrDefault(), CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Format value with invariant culture
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public string FormatValueInv<T>(T value, bool alt = false) where T : unmanaged, IConvertible, IFormattable
        {
            var format = GetFormat<T>(alt);

            if (format == null)
            {
                return value.ToString();
            }
            else
            {
                if (format.IsComposite) 
                    return string.Format(CultureInfo.InvariantCulture, format.GetFormatStringOrDefault(), value);
                else if (format.NeedsConverter) 
                    return format.Spec.Converter(value);
                else 
                    return value.ToString(format.GetFormatStringOrDefault(), CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Format value with invariant culture, by specifying the N sub-values which are expected by the underlying
        /// composite formattable string
        /// </summary>
        /// <typeparam name="T">the sub-values type, parts of the <paramref name="type"/> to be formatted. It's typically
        /// the word type of a dword or a qword type</typeparam>
        /// <param name="type">the type to be formatted</param>
        /// <param name="value1">t</param>
        /// <param name="value2"></param>
        /// <remarks>If performances matter, don't use that. C# (as of now, even with last framework 4.8, string.Format()
        /// is not perf-oriented at all: boxing always occur, as it expects only object arguments)</remarks>
        /// <returns></returns>
        public string FormatValueInv<T>(TypeCode type, T value1, T value2, bool alt = false) 
            where T : IConvertible, IFormattable
        {
            var format = GetFormat(type, alt);

            if (format == null || format.IsComposite == false)
                throw new InvalidOperationException($"Don't know how to format 2 values {typeof(T)} as parts of type {type}");

            return string.Format(format.GetFormatStringOrDefault(), value1, value2);
        }

        #endregion

        private readonly Format[] _formats = new Format[_upperBound];
        private readonly Format[] _altFormats = new Format[_upperBound];

        private static readonly int _upperBound = (int)TypeCode.String;
        private static readonly Format[] _fallbacks = new Format[_upperBound];
    }
 }
