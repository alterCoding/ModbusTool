using System;
using NUnit.Framework;

namespace Modbus.Common.BCL.Tests
{
    [TestFixture]
    public class MarshallerTests
    {
        [Test]
        public void ToBinary()
        {
            float sutval = 1234.567f;

            Marshaller.ToBinary(sutval, out ushort w0, out ushort w1, Endianness.BE);
            Assert.That(w0, Is.EqualTo(0x449A));
            Assert.That(w1, Is.EqualTo(0x5225));

            Marshaller.ToBinary(sutval, out w0, out w1, Endianness.LE);
            Assert.That(w1, Is.EqualTo(0x449A));
            Assert.That(w0, Is.EqualTo(0x5225));

            Marshaller.ToBinary(sutval, out uint dw, Endianness.LE);
            Assert.That(dw, Is.EqualTo(0x449A5225));
            Marshaller.ToBinary(sutval, out dw, Endianness.BE);
            Assert.That(dw, Is.EqualTo(0x5225449A));

            var ws = new ushort[6];
            Marshaller.ToBinary(sutval, new ArraySegment<ushort>(ws, 1, 2), Endianness.BE);
            Assert.That(ws[1], Is.EqualTo(0x449A));
            Assert.That(ws[2], Is.EqualTo(0x5225));

            Marshaller.ToBinary(sutval, new ArraySegment<ushort>(ws, 1, 2), Endianness.LE);
            Assert.That(ws[2], Is.EqualTo(0x449A));
            Assert.That(ws[1], Is.EqualTo(0x5225));
        }

        [Test]
        public void FloatFromBinary()
        {
            uint sutval = 0x449A5225;

            float sutf = Marshaller.FloatFromBinary(sutval, Endianness.native);
            Assert.That(sutf, Is.EqualTo(1234.567f));

            var ws = new ushort[6];
            ws[4] = 0x5225;
            ws[5] = 0x449A;
            sutf = Marshaller.FloatFromBinary(new ArraySegment<ushort>(ws, 4, 2), Endianness.LE);
            Assert.That(sutf, Is.EqualTo(1234.567f));

            ws[1] = 0x5225;
            ws[0] = 0x449A;
            sutf = Marshaller.FloatFromBinary(new ArraySegment<ushort>(ws, 0, 2), Endianness.BE);
            Assert.That(sutf, Is.EqualTo(1234.567f));
        }
    }
}
