using System;
using NUnit.Framework;

namespace Modbus.Common.Tests
{
    using BCL;

    [TestFixture]
    class ModbusRegisterBufferTests
    {
        [Test]
        public void DirectBuffer()
        {
            var data = new ushort[64];
            var buffer = new ModbusRegistersBuffer(data);

            for (int i = 0; i < data.Length; ++i) data[i] = (ushort)i;

            Assert.That(buffer.TryGetRegister(0, out ushort value), Is.True);
            Assert.That(value, Is.EqualTo(0));
            Assert.That(buffer.TryGetRegister(63, out value), Is.True);
            Assert.That(value, Is.EqualTo(63));
            Assert.That(buffer.TryGetRegister(64, out value), Is.False);
            Assert.That(value, Is.EqualTo(0));

            Assert.That(buffer.TrySetRegister(0, 0xffff), Is.True);
            buffer.TryGetRegister(0, out value);
            Assert.That(value, Is.EqualTo(0xffff));

            Assert.That(buffer.TrySetRegisters(0, 0x0123abcd, Endianness.BE), Is.True);
            Assert.That(buffer.TryGetRegisters(0, out uint value2, Endianness.BE), Is.True);
            Assert.That(value2, Is.EqualTo(0x0123abcd));
            Assert.That(buffer.TryGetRegisters(0, out value2, Endianness.LE), Is.True);
            Assert.That(value2, Is.EqualTo(0xabcd0123));
            buffer.TryGetRegister(0, out value);
            Assert.That(value, Is.EqualTo(0x0123));

            Assert.That(buffer.TrySetRegisters(63, 0xffffffff, Endianness.BE), Is.False);
            buffer.TryGetRegister(63, out value);
            Assert.That(value, Is.EqualTo(63));
        }

        [Test]
        public void PartialBuffer()
        {
            var data = new ushort[16];
            var buffer = new ModbusRegistersBuffer(data, 64);

            for (int i = 0; i < data.Length; ++i) data[i] = (ushort)(i+64);

            Assert.That(buffer.TryGetRegister(64, out ushort value), Is.True);
            Assert.That(value, Is.EqualTo(64));

            Assert.That(buffer.TryGetRegisters(78, out uint value2, Endianness.BE), Is.True);
            Assert.That(value2, Is.EqualTo(0x004e004f));

            Assert.That(buffer.TryGetRegisters(79, out value2, Endianness.BE), Is.False);
        }
    }
}
