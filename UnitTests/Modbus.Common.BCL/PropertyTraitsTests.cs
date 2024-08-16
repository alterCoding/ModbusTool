using System;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    class PropertyTraitsTests
    {
        [Test]
        public void NumericTraits()
        {
            Assert.That(PrimitiveTraits<uint>.IsUnsigned, Is.True);
            Assert.That(PrimitiveTraits<int>.IsUnsigned, Is.False);
            Assert.That(PrimitiveTraits<uint>.IsArithmetic, Is.True);
            Assert.That(PrimitiveTraits<uint>.IsFloatingPoint, Is.False);
            Assert.That(PrimitiveTraits<float>.IsFloatingPoint, Is.True);
            Assert.That(PrimitiveTraits<double>.IsIntegral, Is.False);
            Assert.That(PrimitiveTraits<int>.IsInteger, Is.True);
            Assert.That(PrimitiveTraits<decimal>.IsFloatingPoint, Is.True);
            Assert.That(PrimitiveTraits<decimal>.IsSigned, Is.True);

            //debate is opened ...
            Assert.That(PrimitiveTraits<bool>.IsIntegral, Is.True);
            Assert.That(PrimitiveTraits<bool>.IsInteger, Is.False);
        }

        [Test]
        public void TypeTraits()
        {
            Assert.That(TypeCode.Int16.ToPrimitiveType(), Is.EqualTo(typeof(short)));
            Assert.That(TypeCode.Boolean.ToPrimitiveType(), Is.EqualTo(typeof(bool)));
            Assert.That(TypeCode.Decimal.ToPrimitiveType(), Is.EqualTo(typeof(decimal)));
            Assert.That(() => TypeCode.String.ToPrimitiveType(), Throws.TypeOf<InvalidCastException>());

            Assert.That(typeof(uint).ToggleSigned(), Is.EqualTo(typeof(int)));
            Assert.That(typeof(sbyte).ToggleSigned(), Is.EqualTo(typeof(byte)));
            Assert.That(() => typeof(bool).ToggleSigned(), Throws.InvalidOperationException);
            Assert.That(() => typeof(decimal).ToggleSigned(), Throws.InvalidOperationException);
        }
    }

}
