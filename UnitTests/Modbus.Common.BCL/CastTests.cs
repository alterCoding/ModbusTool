using System;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    class CastTests
    {
        [Test]
        public void CastPrimitives()
        {
            Assert.Throws<OverflowException>(() => Cast<uint>.Checked(int.MinValue));
            uint u32 = Cast<uint>.UnChecked(int.MinValue);
            Assert.That(u32, Is.EqualTo(unchecked((uint)int.MinValue)));
            Assert.That(Cast<int>.UnChecked(u32), Is.EqualTo(int.MinValue));

            Assert.Throws<OverflowException>(() => Cast<byte>.Checked(short.MinValue+1));
            Assert.That(Cast<byte>.UnChecked(short.MinValue+1), Is.EqualTo(0x1));

            //memo purpose: we should not be happy with the C# FPT numbers processing
            //check() here does nothing at all ...  to test number.infinity is the sole way to cope with this kind of
            //bullshit. As a result checked() unchecked() ... it's the same useless thing (C# only takes into acocunt
            //the arithmetic overflow with unsigned/signed integral type)
            Assert.That(float.IsInfinity(Cast<float>.Checked(double.MaxValue)), Is.True);
        }
    }
}
