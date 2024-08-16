using System;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    /// <summary>
    /// ConvertClassGeneric tests
    /// </summary>
    [TestFixture]
    class ConvertGenericTests
    {
        [Test]
        public void ToStringWithBaseConv()
        {
            var convert = ConvertClassGeneric.Default;

            //useless call, aside from testing
            Assert.That(convert.ToString(byte.MaxValue, 10), Is.EqualTo(byte.MaxValue.ToString()));

            Assert.That(convert.ToString(byte.MaxValue, 16), Is.EqualTo("ff"));
            Assert.That(convert.ToString(byte.MinValue, 16), Is.EqualTo("0"));
            Assert.That(convert.ToString(sbyte.MinValue, 16), Is.EqualTo("80"));
            Assert.That(convert.ToString(ushort.MaxValue, 16), Is.EqualTo("ffff"));

            Assert.That(convert.ToString(0x4, 2), Is.EqualTo("100"));
            Assert.That(convert.ToString(short.MinValue, 2), Is.EqualTo("1000000000000000"));

            Assert.That(convert.ToString((ushort)8, 8), Is.EqualTo("10"));

            //feel free to continue .....

            //limitations on the supported types ... due to underlying dot.net implementation of Convert class ---

            //NOTE: nunit runner is unable to catch an Exception from TypeInitlizationException .. which inherit from
            //Exception though ! so we wrap two assert.that ... to get it work
            Assert.That(Assert.Throws<TypeInitializationException>(() => convert.ToString(float.MaxValue, 16))
                .InnerException, Is.InstanceOf<InvalidCastException>());
        }

        [Test]
        public void FromStringWithBaseConv()
        {
            var convert = ConvertClassGeneric.Default;

            Assert.That(convert.FromString<byte>("255", 10), Is.EqualTo(byte.MaxValue));
            Assert.That(convert.FromString<sbyte>("-128", 10), Is.EqualTo(sbyte.MinValue));
            Assert.That(convert.FromString<ushort>("0xffff", 16), Is.EqualTo(ushort.MaxValue));
            Assert.That(convert.FromString<short>("ffff", 16), Is.EqualTo(-1));

            //raise from underlying
            Assert.That(() => convert.FromString<sbyte>("255", 10), Throws.InstanceOf<OverflowException>());
            Assert.That(Assert.Throws<TypeInitializationException>(() => convert.FromString<float>("1.5", 10))
                .InnerException, Is.InstanceOf<InvalidCastException>());
        }

    }
}

