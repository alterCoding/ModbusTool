using System;

namespace Modbus.Common
{
    using BCL;

    /// <summary>
    /// Modbus register(s) low-level SET operations
    /// </summary>
    internal interface IMBDataWriter
    {
        /// <summary>
        /// Set a single native 16bit register
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value">The WORD value. The true data type falls within the application scope, it may be a 16
        /// or a 8 bit value, and the conveyed value type may be signed, since the signed/unsigned traits is not a 
        /// modbus or buffer topic</param>
        /// <returns></returns>
        bool TrySetRegister(ushort addr, ushort value);

        /// <summary>
        /// Set a virtual 32 bit register as 2 contiguous native registers, according to the supplied endianness
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value">The DWORD vaule as 32bit value<br/>
        /// The conveyed value type may be signed as the signed/unsigned is not a modbus nor a buffer topic. In the same
        /// way, the underlying value could be a float32</param>
        /// <param name="endianness">The target encoding INT32 or FLOAT32 endianness</param>
        /// <returns></returns>
        bool TrySetRegisters(ushort addr, uint value, Endianness endianness);
    }

    /// <summary>
    /// Modbus register(s) low-level GET operations
    /// </summary>
    internal interface IMBDataReader
    {
        /// <summary>
        /// GET a single 16bit register
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value">The WORD value to be retrieved (8/16 bit, signed or unsigned integer). The final
        /// data type falls within the application scope</param>
        /// <returns></returns>
        bool TryGetRegister(ushort addr, out ushort value);

        /// <summary>
        /// GET a virtual 32 bit register, as a read of 2 contiguous 16bit registers
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value">The DWORD value to be retrieved (32bit, signed or unsigned integer, or floating point).
        /// The final data type falls within the application scope</param>
        /// <param name="endianness">The expected source INT32 or FLOAT32 endianness</param>
        /// <returns></returns>
        bool TryGetRegisters(ushort addr, out uint value, Endianness endianness);
    }

    /// <summary>
    /// Is a trivial wrapper onto an underlying word buffer (ushort), with get/set word(s) methods
    /// </summary>
    internal class ModbusRegistersBuffer : IMBDataWriter, IMBDataReader
    {
        /// <summary>
        /// </summary>
        /// <param name="data">The underlying buffer or sub part</param>
        /// <param name="offset">The offset to be applied on requested or supplied addresses. It's the offset of the
        /// buffer origin <paramref name="data"/> into the underlying whole buffer</param>
        public ModbusRegistersBuffer(ushort[] data, ushort offset = 0)
        {
            _data = data;
            _offset = offset;
        }

        private bool checkAddress(ref ushort addr, int count = 1)
        {
            if (addr - _offset + count > _data.Length) return false;

            addr -= _offset;
            return true;
        }

        public bool TrySetRegister(ushort addr, ushort value)
        {
            if (!checkAddress(ref addr)) return false;

            _data[addr] = value;

            return true;
        }

        public bool TryGetRegister(ushort addr, out ushort value)
        {
            if (!checkAddress(ref addr))
            {
                value = default;
                return false;
            }

            value = _data[addr];
            return true;
        }

        public bool TrySetRegisters(ushort addr, uint value, Endianness endianness)
        {
            if (!checkAddress(ref addr, 2)) return false;

            endianness = endianness.ShouldBeDefined();

            ushort lsw = (ushort)value;
            ushort msw = (ushort)(value >> 16);

            if(endianness == Endianness.BE)
            {
                _data[addr] = msw;
                _data[addr + 1] = lsw;
                return true;
            }
            else if(endianness == Endianness.LE)
            {
                _data[addr] = lsw;
                _data[addr + 1] = msw;
                return true;
            }

            //unreached
            return false;
        }

        public bool TryGetRegisters(ushort addr, out uint value, Endianness endianness)
        {

            if (!checkAddress(ref addr, 2))
            {
                value = default;
                return false;
            }

            endianness = endianness.ShouldBeDefined();
            ushort lsw = 0, msw = 0;

            if(endianness == Endianness.BE)
            {
                msw = _data[addr];
                lsw = _data[addr + 1];
            }
            else if(endianness == Endianness.LE)
            {
                lsw = _data[addr];
                msw = _data[addr + 1];
            }

            value = lsw | (uint)msw << 16;
            return true;
        }

        public ushort[] Data => _data;

        private readonly ushort _offset;

        private readonly ushort[] _data;
    }
}
