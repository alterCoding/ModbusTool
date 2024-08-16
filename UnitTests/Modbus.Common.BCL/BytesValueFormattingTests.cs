using System;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    class BytesValueFormattingTests 
    {
        [Test]
        public void DefaultValue()
        {
            var v = new FormattedValue<ushort>();
            Assert.That(v.IsEmpty, Is.True);
            Assert.That(v, Is.EqualTo(FormattedValue<ushort>.NULL));
        }

        [Test]
        public void Format()
        {
            var v = FormattedValue.Create((ushort)0x0123, ArithmeticValueFormat.hexa);
            Assert.That(v.Text, Is.EqualTo("0x0123"));
            Assert.That(v.Value, Is.EqualTo(0x0123));
            Assert.That(v.IsEmpty, Is.False);

            v = FormattedValue.Create((ushort)0xff, ArithmeticValueFormat.@decimal);
            Assert.That(v.Text, Is.EqualTo("255"));
        }

        [Test]
        public void ParseUInt16()
        {
            ///see <see cref="FormattedValue.Default"/> for the default formatting rules
            var formatting = new BytesValueParsing<ushort>();

            Assert.That(formatting.TryParse("0xffff", isHexDefault: true, out var v), Is.True);
            Assert.That(v.Value, Is.EqualTo(ushort.MaxValue));
            //an explicit hexa value ... is still successfully parsed as hexa, although the default flag was inaccurate
            Assert.That(formatting.TryParse("0xFFFF", isHexDefault: false, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(ushort.MaxValue));

            Assert.That(formatting.TryParse("1000", isHexDefault: false, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(1000));
            Assert.That(formatting.TryParse("1000", isHexDefault: true, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(0x1000));

            Assert.That(formatting.TryParse(string.Empty, isHexDefault: true, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(0));
            Assert.That(formatting.TryParse(string.Empty, isHexDefault: false, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(0));
            Assert.That(formatting.TryParse("0x", isHexDefault: false, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(0));
               
            var format = FormatOptions.Default(ArithmeticValueFormat.binary);
            Assert.That(formatting.TryParse("0b10", format, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(2));
            Assert.That(v.Text, Is.EqualTo("0b0000000000000010")); //has been reformated according to the formatting rules

            format = new FormatOptions(ArithmeticValueFormat.binary, useAlt:true);//update format
            Assert.That(formatting.TryParse("0b101", format, out v), Is.False);
            Assert.That(formatting.TryParse("101", format, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(5));
            Assert.That(v.Text, Is.EqualTo("0000000000000101")); //format has been changed (no prefix anymore)
        }

        [Test]
        public void ParseInt16()
        {
            var formatting = new BytesValueParsing<short>();

            var format = FormatOptions.Default(ArithmeticValueFormat.@decimal);
            Assert.That(formatting.TryParse("-1", format, out var v), Is.True);
            Assert.That(v.Value, Is.EqualTo(-1));

            Assert.That(formatting.TryParse(string.Empty, format, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(0));
            Assert.That(formatting.TryParse("-", format, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(0));
            format = new FormatOptions(ArithmeticValueFormat.@decimal, emptyIsZero: false);
            Assert.That(formatting.TryParse("-", format, out v), Is.False);

            format = FormatOptions.Default(ArithmeticValueFormat.hexa);
            Assert.That(formatting.TryParse("0xffff", format, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(-1));

            Assert.That(formatting.TryParse("8000", isHexDefault: true, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(short.MinValue));
        }

        [Test]
        public void ParseInt8()
        {
            var formatting = new BytesValueParsing<sbyte>();

            var format = FormatOptions.Default(ArithmeticValueFormat.@decimal);
            Assert.That(formatting.TryParse("-1", format, out var v), Is.True);
            Assert.That(v.Value, Is.EqualTo(-1));

            Assert.That(formatting.TryParse("80", isHexDefault: true, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(sbyte.MinValue));

            format = FormatOptions.Default(ArithmeticValueFormat.binary);
            Assert.That(formatting.TryParse("0b10000000", format, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(sbyte.MinValue));
        }

        [Test]
        public void ParseInt32()
        {
            var formatting = new BytesValueParsing<int>();

            var format = FormatOptions.Default(ArithmeticValueFormat.@decimal);
            Assert.That(formatting.TryParse($"{int.MaxValue}", format, out var v), Is.True);
            Assert.That(v.Value, Is.EqualTo(int.MaxValue));

            format = FormatOptions.Default(ArithmeticValueFormat.hexa);
            Assert.That(formatting.TryParse("0xffffffff", format, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(-1));

            Assert.That(formatting.TryParse("80000000", isHexDefault: true, out v), Is.True);
            Assert.That(v.Value, Is.EqualTo(int.MinValue));

        }
    }
}
