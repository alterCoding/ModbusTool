using System;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    class ValueFormattingTests
    {
        [Test]
        public void FormatFloat()
        {
            var formatting = makeFormatting();

            float sutval = 1234.567f;
            //uint sutbin = 0x449A5225;
            Assert.That(formatting.FormatInvariant(sutval, ArithmeticValueFormat.@decimal), Is.EqualTo("1234.567"));
            Assert.That(formatting.FormatInvariant(sutval, new FormatOptions(ArithmeticValueFormat.@decimal, maxLen:6)), Is.EqualTo("1234.57"));
            Assert.That(formatting.FormatInvariant(sutval, ArithmeticValueFormat.hexa), Is.EqualTo("0x449A5225"));
            //0xFFFFFFFF
            Assert.That(formatting.GetMaxLengthOutput(sutval.GetTypeCode(), ArithmeticValueFormat.hexa), Is.EqualTo(10));

            sutval = 123456789f; //best accurate representation is 123456792
            Assert.That(formatting.FormatInvariant(sutval, ArithmeticValueFormat.@decimal), Is.EqualTo("1.234568E+08"));
            Assert.That(formatting.FormatInvariant(sutval, new FormatOptions(ArithmeticValueFormat.@decimal, maxLen:9)), Is.EqualTo("1.23457e+08"));
            //-d.ddddddE+dd (G7)
            Assert.That(formatting.GetMaxLengthOutput(sutval.GetTypeCode(), ArithmeticValueFormat.@decimal), Is.EqualTo(13));
            Assert.That(formatting.GetMaxLengthOutput(sutval.GetTypeCode(), new FormatOptions(ArithmeticValueFormat.@decimal, useAlt:true)), Is.EqualTo(12));

            //with the best precision ----------
            formatting = new ValueFormatting(9, 17);
            Assert.That(formatting.FormatInvariant(sutval, ArithmeticValueFormat.@decimal), Is.EqualTo("123456792"));
            Assert.That(formatting.GetMaxLengthOutput(sutval.GetTypeCode(), ArithmeticValueFormat.@decimal), Is.EqualTo(15));
        }

        [Test]
        public void FormatShort()
        {
            var formatting = makeFormatting();

            ushort sutval = 65535;
            Assert.That(formatting.FormatInvariant(sutval, ArithmeticValueFormat.@decimal), Is.EqualTo("65535"));
            Assert.That(formatting.FormatInvariant(sutval, ArithmeticValueFormat.hexa), Is.EqualTo("0xFFFF"));
            Assert.That(formatting.FormatInvariant(sutval, ArithmeticValueFormat.binary), Is.EqualTo("0b1111111111111111"));
            Assert.That(formatting.FormatInvariant(sutval, new FormatOptions(ArithmeticValueFormat.binary, useAlt:true)), Is.EqualTo("1111111111111111"));

            //FFFFh
            Assert.That(formatting.GetMaxLengthOutput(sutval.GetTypeCode(), new FormatOptions(ArithmeticValueFormat.hexa, useAlt:true)), Is.EqualTo(5));
        }

        [Test]
        public void FormatFallback()
        {
            var formatting = makeFormatting();

            //if type is not registered, we receive a fallback of (by convention) decimal output type
            Assert.That(formatting.GetMaxLengthOutput(TypeCode.UInt64, ArithmeticValueFormat.@decimal), Is.EqualTo(ulong.MaxValue.ToString().Length));

            //we don't want to assume how a default hexa value should be formatted ... please register the ad'hoc format
            Assert.That(formatting.GetMaxLengthOutput(TypeCode.UInt64, ArithmeticValueFormat.hexa), Is.EqualTo(0));
        }

        private ValueFormatting makeFormatting() => new ValueFormatting(7, 15, 6, 12);
    }
}
