using System;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    class PrimitiveFormatterTests
    {
        [Test]
        public void MakeSpecByFormatString()
        {
            //e.g if we plan to format a double with 4*16bit
            {
                var format = PrimitiveFormatter.Format<double>.HexaN(4, upper: true, padding: 4);
                Assert.That(format.IsComposite, Is.True);
                Assert.That(format.GetFormatStringOrDefault(), Is.EqualTo("0x{0:X04}{1:X04}{2:X04}{3:X04}"));
                Assert.That(format.FormatProperties.Prefix, Is.EqualTo("0x"));
            }

            {
                var format = PrimitiveFormatter.Format<ushort>.ShortHexa();
                Assert.That(format.IsComposite, Is.False);
                Assert.That(format.GetFormatStringOrDefault(), Is.EqualTo("x"));
            }

            {
                var format = PrimitiveFormatter.Format<double>.FloatingPoint();
                Assert.That(format.IsComposite, Is.False);
                Assert.That(format.GetFormatStringOrDefault(), Is.EqualTo("G"));
                format = PrimitiveFormatter.Format<double>.FloatingPoint(6);
                Assert.That(format.GetFormatStringOrDefault(), Is.EqualTo("G6"));
            }

            {
                var format = PrimitiveFormatter.Format<byte>.User("00# units", ArithmeticValueFormat.@decimal);
                Assert.That(format.IsUserFormat, Is.True);
                format = PrimitiveFormatter.Format<byte>.User("{0:x02}h", ArithmeticValueFormat.hexa);
                Assert.That(format.IsComposite, Is.True);
            }
        }

        [Test]
        public void MakeSpecByConversion()
        {
            var format = PrimitiveFormatter.Format<byte>.Binary();
            Assert.That(format.IsComposite, Is.False);
            Assert.That(format.NeedsConverter, Is.True);
            Assert.That(format.GetFormatStringOrDefault(), Is.Empty);
            Assert.That(format.FormatProperties.Prefix, Is.EqualTo("0b"));
        }

        [Test]
        public void GetFormat()
        {
            var formatter = makeDefaultFormatter();

            Assert.That(formatter.GetFormat<ushort>().GetFormatStringOrDefault(), Is.EqualTo("0x{0:X04}"));
            Assert.That(formatter.GetFormat<float>().GetFormatStringOrDefault(), Is.EqualTo("0x{0:X04}{1:X04}"));
            Assert.That(formatter.GetFormat<float>(alt:true).GetFormatStringOrDefault(), Is.EqualTo("G4"));
            Assert.That(formatter.GetFormat<double>(), Is.Null);
            Assert.That(formatter.GetFormatOrFallback<double>().GetFormatStringOrDefault(), Is.EqualTo("G"));
            Assert.That(formatter.GetFormatOrFallback<double>().KindOfFormat, Is.EqualTo(ArithmeticValueFormat.@decimal));
        }

        [Test]
        public void Format()
        {
            var formatter = makeDefaultFormatter();

            Assert.That(formatter.FormatValueInv((ushort)255), Is.EqualTo("0x00FF"));
            Assert.That(formatter.FormatValueInv((ushort)255, alt: true), Is.EqualTo("255"));
            Assert.That(formatter.FormatValueInv(TypeCode.Single, 0x449a, 0x5225), Is.EqualTo("0x449A5225"));
            Assert.That(formatter.FormatValueInv(12345.6f, alt:true), Is.EqualTo("1.235E+04"));
            Assert.That(formatter.FormatValueInv(byte.MaxValue), Is.EqualTo("0b11111111"));
            Assert.That(formatter.FormatValueInv((byte)1), Is.EqualTo("0b00000001"));
        }

        [Test]
        public void MaxLength()
        {
            var formatter = makeDefaultFormatter();

            //0xffff
            Assert.That(formatter.GetFormat<ushort>().MaxLength, Is.EqualTo(6));
            //65535
            Assert.That(formatter.GetFormat<ushort>(alt:true).MaxLength, Is.EqualTo(5));
            //0xffffffff
            Assert.That(formatter.GetFormat<float>().MaxLength, Is.EqualTo(10));
            //d.ddddE+dd
            Assert.That(formatter.GetFormat<float>(alt:true).MaxLength, Is.EqualTo(10));

            //if format hasn't been defined, but fallback resolving offer a decent result
            Assert.That(formatter.GetFormatOrFallback<uint>().MaxLength, Is.EqualTo(uint.MaxValue.ToString().Length));
            Assert.That(formatter.GetFormatOrFallback<int>().MaxLength, Is.EqualTo(int.MinValue.ToString().Length));
            Assert.That(formatter.GetFormatOrFallback<ulong>().MaxLength, Is.EqualTo(ulong.MaxValue.ToString().Length));
            Assert.That(formatter.GetFormatOrFallback<long>().MaxLength, Is.EqualTo(long.MinValue.ToString().Length));
            Assert.That(formatter.GetFormatOrFallback<long>().KindOfFormat, Is.EqualTo(ArithmeticValueFormat.@decimal));
            //-d.ddddddddddddddE+ddd
            Assert.That(formatter.GetFormatOrFallback<double>().MaxLength, Is.EqualTo(double.MinValue.ToString().Length));

            //0bdddddddd
            Assert.That(formatter.GetFormat<byte>().MaxLength, Is.EqualTo(10));
        }

        /// <summary>
        /// This formatter defines an hexa format for ushort and float. Alternative formats are defined as simple
        /// regular decimal outputs
        /// </summary>
        /// <returns></returns>

        private PrimitiveFormatter makeDefaultFormatter()
        {
            var formatter = new PrimitiveFormatter
            (
                PrimitiveFormatter.Format<ushort>.Hexa(upper: true, padding: 4),
                PrimitiveFormatter.Format<float>.HexaN(2, upper: true, padding: 4),
                PrimitiveFormatter.Format<byte>.Binary(8)
            )
            .WithAlternative
            (
                PrimitiveFormatter.Format<ushort>.Integer(),
                PrimitiveFormatter.Format<float>.FloatingPoint(4)
            );

            return formatter;
        }
    }
}
