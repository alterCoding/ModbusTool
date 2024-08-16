using System;
using System.Globalization;
using NUnit.Framework;

namespace Modbus.Common.Tests
{
    using BCL;

    [TestFixture]
    class MBDataItemTests
    {
        [Test]
        public void BadNativeRegisterConstruct()
        {
            Assert.That(Assert.Throws<TypeInitializationException>(
                () => new MBNativeDataItem<uint>(0x1000, 0, ArithmeticValueFormat.hexa))
                .InnerException, Is.InstanceOf<NotSupportedException>());
        }

        [Test]
        public void NativeRegisters()
        {
            //create an empty value with a default format
            var reg16 = new MBNativeDataItem<ushort>(0x10, 0, ArithmeticValueFormat.hexa);

            Assert.That(reg16.Address, Is.EqualTo(0x10));
            Assert.That(reg16.Value.IsEmpty, Is.True);
            Assert.That(reg16.Formatting, Is.SameAs(FormatOptions.Default(ArithmeticValueFormat.hexa)));

            //update raw value
            reg16.Update(0xc00);
            Assert.That(reg16.Value.Value, Is.EqualTo(0xc00));
            Assert.That(reg16.Value.Text, Is.EqualTo("0x0C00"));

            //update raw value and change format
            Assert.That(reg16.Update(0xa, FormatOptions.Default(ArithmeticValueFormat.@decimal)), Is.EqualTo("10"));

            bool updated = false;
            reg16.OnValueChanged += (sender, evt) => { Assert.That(sender, Is.SameAs(reg16)); updated = true; };

            //update while parsing
            Assert.That(reg16.TryParse("11"), Is.True);
            Assert.That(reg16.Value.Value, Is.EqualTo(0xb));
            Assert.That(reg16.Text, Is.EqualTo("11"));
            Assert.That(updated, Is.True);

            //parsing with new format
            Assert.That(reg16.TryParse("0xc", FormatOptions.Default(ArithmeticValueFormat.hexa)), Is.True);
            Assert.That(reg16.Text, Is.EqualTo("0x000C"));

            //parsing failure
            Assert.That(reg16.TryParse("zz"), Is.False);
            Assert.That(reg16.Value.Value, Is.EqualTo(12)); //no change

            //update format only
            updated = false;
            //no change (raw value)
            reg16.Update(12, FormatOptions.Default(ArithmeticValueFormat.binary));
            Assert.That(reg16.Value.Text, Is.EqualTo("0b0000000000001100")); //has been reformated
            Assert.That(updated, Is.False);

            //read from buffer
            var buffer = new ModbusRegistersBuffer(new ushort[64]);
            buffer.Data[0x10] = 0xffff;
            Assert.That(reg16.ReadFrom(buffer, Endianness.undefined), Is.True);
            Assert.That(reg16.Value.Value, Is.EqualTo(ushort.MaxValue));
        }

        [Test]
        public void NativeSignedRegisters()
        {
            var value = FormattedValue.Create(short.MaxValue, ArithmeticValueFormat.@decimal);
            var sreg16 = MBDataItem.Create(0x10, 0, value);

            Assert.That(sreg16.Value.Text, Is.EqualTo(short.MaxValue.ToString()));

            //update raw value
            sreg16.Update(short.MinValue);
            Assert.That(sreg16.Value.Value, Is.EqualTo(short.MinValue));
            Assert.That(sreg16.Value.Text, Is.EqualTo(short.MinValue.ToString()));

            //update raw value and change format
            Assert.That(sreg16.Update(-1, FormatOptions.Default(ArithmeticValueFormat.hexa)), Is.EqualTo("0xFFFF"));

            //update while parsing
            Assert.That(sreg16.TryParse("-11", FormatOptions.Default(ArithmeticValueFormat.@decimal)), Is.True);
            Assert.That(sreg16.Value.Value, Is.EqualTo(-11));
            Assert.That(sreg16.Text, Is.EqualTo("-11"));

            //read from buffer
            var buffer = new ModbusRegistersBuffer(new ushort[64]);
            buffer.Data[0x10] = 0x8000;
            Assert.That(sreg16.ReadFrom(buffer, Endianness.undefined), Is.True);
            Assert.That(sreg16.Value.Value, Is.EqualTo(short.MinValue));
        }

        [Test]
        public void Extended32Registers()
        {
            var buffer = new ModbusRegistersBuffer(new ushort[64]);

            //mapping UINT32 ---------------------------------------

            var regU32 = new MBVirtual32DataItem<uint>(0x10, 0, new FormatOptions(ArithmeticValueFormat.hexa, useAlt:true));

            regU32.Update(uint.MaxValue);
            Assert.That(regU32.Value.Value, Is.EqualTo(uint.MaxValue));
            Assert.That(regU32.Value.Text, Is.EqualTo($"0x{uint.MaxValue.ToString("X8")}"));

            Assert.That(regU32.TryParse("131071", FormatOptions.Default(ArithmeticValueFormat.@decimal)), Is.True);
            Assert.That(regU32.Value.Value, Is.EqualTo(0x0001ffff));
            Assert.That(regU32.Value.Text, Is.EqualTo("131071"));

            Assert.That(regU32.WriteTo(buffer, Endianness.BE), Is.True);
            Assert.That(buffer.Data[0x10], Is.EqualTo(0x0001));
            Assert.That(buffer.Data[0x11], Is.EqualTo(0xffff));

            buffer.Data[0x10] = 0x00ab;
            buffer.Data[0x11] = 0xcdef;
            Assert.That(regU32.ReadFrom(buffer, Endianness.BE), Is.True);
            Assert.That(regU32.Value.Value, Is.EqualTo(0x00abcdef));

            //mapping INT32 ---------------------------------------

            var regI32 = new MBVirtual32DataItem<int>(0x20, 0, new FormatOptions(ArithmeticValueFormat.hexa, useAlt: true));

            regI32.Update(int.MinValue+1);
            Assert.That(regI32.Value.Value, Is.EqualTo(int.MinValue+1));
            Assert.That(regI32.Value.Text, Is.EqualTo("0x80000001"));

            Assert.That(regI32.WriteTo(buffer, Endianness.BE), Is.True);
            Assert.That(buffer.Data[0x20], Is.EqualTo(0x8000));
            Assert.That(buffer.Data[0x21], Is.EqualTo(0x0001));

            Assert.That(regI32.TryParse("-1", FormatOptions.Default(ArithmeticValueFormat.@decimal)), Is.True);
            Assert.That(regI32.Value.Value, Is.EqualTo(-1));

            buffer.Data[0x20] = 0x0002;
            buffer.Data[0x21] = 0x8000;
            Assert.That(regI32.ReadFrom(buffer, Endianness.LE), Is.True);
            Assert.That(regI32.Value.Value, Is.EqualTo(int.MinValue+2));

            //mapping FLOAT32 ----------------------------------------

            var regF32 = new MBVirtual32DataItem<float>(0x0, 0, ArithmeticValueFormat.@decimal);
            regF32.Update(1.4f);
            Assert.That(regF32.Value.Value, Is.EqualTo(1.4f));
            Assert.That(regF32.Value.Text, Is.EqualTo("1.4"));
            regF32.Update(1.4f, FormatOptions.Default(ArithmeticValueFormat.hexa));
            Assert.That(regF32.Value.Text, Is.EqualTo("0x3FB33333"));

            Assert.That(regF32.WriteTo(buffer, Endianness.BE), Is.True);
            Assert.That(buffer.Data[0x0], Is.EqualTo(0x3FB3));
            Assert.That(buffer.Data[0x1], Is.EqualTo(0x3333));
            Assert.That(regF32.WriteTo(buffer, Endianness.LE), Is.True);
            Assert.That(buffer.Data[0x1], Is.EqualTo(0x3FB3));
            Assert.That(buffer.Data[0x0], Is.EqualTo(0x3333));

            //reminder: G7 is default precision, but roundtrip needs more
            string max = float.MaxValue.ToString("G9", NumberFormatInfo.InvariantInfo);;
            Assert.That(regF32.TryParse(max, FormatOptions.Default(ArithmeticValueFormat.@decimal)), Is.True);
            Assert.That(regF32.Value.Value, Is.EqualTo(float.MaxValue));
            Assert.That(regF32.WriteTo(buffer, Endianness.BE), Is.True);
            Assert.That(buffer.Data[0x0], Is.EqualTo(0x7F7F)); //max exponent: 2e127 and max mantissa
            Assert.That(buffer.Data[0x1], Is.EqualTo(0xFFFF)); //max mantissa

            buffer.Data[0x0] = 0xffff;
            buffer.Data[0x1] = 0xff7f;
            Assert.That(regF32.ReadFrom(buffer, Endianness.LE), Is.True);
            Assert.That(regF32.Value.Value, Is.EqualTo(float.MinValue));
        }
    }
}
