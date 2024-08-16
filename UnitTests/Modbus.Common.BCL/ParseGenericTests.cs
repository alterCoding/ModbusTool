using System;
using NUnit.Framework;
using System.Globalization;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    class ParseGenericTests
    {
        [Test]
        public void NativeTryParse()
        {
            var parser = ParseGeneric.Default;

            Assert.That(parser.TryParseHexa("ffff", out ushort uvalue), Is.True);
            Assert.That(uvalue, Is.EqualTo(0xffff));

            Assert.That(parser.TryParseHexa("ffff", out short svalue), Is.True);
            Assert.That(svalue, Is.EqualTo(-1));

            Assert.That(parser.TryParseDecimal("1.5", out float fvalue), Is.True);
            Assert.That(fvalue, Is.EqualTo(1.5f));
            Assert.That(parser.TryParseDecimal("15", out int value), Is.True);
            Assert.That(value, Is.EqualTo(15));

            //yes ... c# cannot convert back its proper default representation, so we have to help it
            string dbl_min = double.MinValue.ToString("G17", NumberFormatInfo.InvariantInfo);
            Assert.That(parser.TryParseFloatingPoint(dbl_min, out double dvalue), Is.True);
            Assert.That(dvalue, Is.EqualTo(double.MinValue));

            //it fails because ushort.TryParse() has decided that allowHexSpecifier enables only hex digits (but not any
            //standard prefix ... use Convert.ToUint16() or cope as yourself with the prefixes ...
            Assert.That(parser.TryParseHexa("0xffff", out ushort _), Is.False);

            //float.TryParse(text, hexa) is not defined
            Assert.That(() => parser.TryParseHexa("ffff", out float _), Throws.ArgumentException);
        }

    }
}
