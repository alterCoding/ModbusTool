using System;
using System.Globalization;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    class FloatingPointFormattingTests
    {
        [Test]
        public void Format()
        {
            var v = FormattedValue.Create(1.4f, ArithmeticValueFormat.@decimal);
            Assert.That(v.Text, Is.EqualTo("1.4"));
            Assert.That(v.Value, Is.EqualTo(1.4f));
            Assert.That(v.IsEmpty, Is.False);

            v = FormattedValue.Create(1.4f, ArithmeticValueFormat.hexa);
            Assert.That(v.Text, Is.EqualTo("0x3FB33333"));
        }

        [Test]
        public void ParseFloat32()
        {
            var formatting = new FloatingPointParsing<float>();

            Assert.That(formatting.TryParse("1.4", out var value), Is.True);
            Assert.That(value.Value, Is.EqualTo(1.4f));

            string sfmax = float.MaxValue.ToString($"G{PrimitiveFormatter._floatRoundTripPrecision}", NumberFormatInfo.InvariantInfo);
            string sfmin = float.MinValue.ToString($"G{PrimitiveFormatter._floatRoundTripPrecision}", NumberFormatInfo.InvariantInfo);

            Assert.That(formatting.TryParse(sfmax, out value), Is.True);
            Assert.That(value.Value, Is.EqualTo(float.MaxValue));
            Assert.That(formatting.TryParse(sfmin, out value), Is.True);
            Assert.That(value.Value, Is.EqualTo(float.MinValue));

        }
    }
}
