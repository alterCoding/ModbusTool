using System;

namespace Modbus.Common.BCL
{
    /// <summary>
    /// Marshalling operations for floating point types
	/// <para>
    /// Scope: we only cope with words (16bit) endianness topic. The byte-level is related to the low level area, which
    /// is managed in the communication library (i.e not here). Typically, the byte level remains BigEndian, according
	/// to the modbus specification.<br/>
	/// Besides, IEEE 754 does not specify anything about the endianness topic, which refers to the implementation side
	/// </para>
    /// </summary>
    /// @todo When the project will be ready to be ported on .net instead of framework, rewrite methods with the new 
    /// span/memory/unsafe/marshalling features instead of ineffective BitConverter
    public static class Marshaller
	{
		/// <summary>
		/// Get 2 contiguous 16bit from a Single in network order
		/// </summary>
		/// <param name="value"></param>
		/// <param name="bin">the target buffer</param>
		/// <param name="endian">the target endianness</param>
		public static void ToBinary(float value, ArraySegment<ushort> bin, Endianness endian = Endianness.native)
		{
			if (bin.Count != 2)
				throw new ArgumentException(
					$"Unable to marshall float to binary. Expecting a target buffer of size 2, but was {bin.Count}");

			bool needs_swap = !endian.IsPlatform();
			int w0 = bin.Offset + (needs_swap ? 1 : 0);
			int w1 = bin.Offset + (needs_swap ? 0 : 1);

			var bytes = BitConverter.GetBytes(value);

			bin.Array[w0] = (ushort)(bytes[0] | bytes[1] << 8);
			bin.Array[w1] = (ushort)(bytes[2] | bytes[3] << 8);
		}

		/// <summary>
		/// Get 2 contiguous 16bits from a Single in network order
		/// </summary>
		/// <param name="value"></param>
		/// <param name="word0">if <paramref name="endian"/> is BE, returns the original MSW (LE the LSW)</param>
		/// <param name="word1">if <paramref name="endian"/> is BE, returns the original LSW (LE the MSW)</param>
		/// <param name="endian">the target endianness</param>
		public static void ToBinary(float value, out ushort word0, out ushort word1, Endianness endian = Endianness.native)
        {
			bool needs_swap = !endian.IsPlatform();
			int b0 = needs_swap ? 2 : 0;
			int b2 = needs_swap ? 0 : 2;

			var bytes = BitConverter.GetBytes(value);

			word0 = (ushort)(bytes[b0] | bytes[b0 + 1] << 8);
			word1 = (ushort)(bytes[b2] | bytes[b2 + 1] << 8);
        }

		/// <summary>
		/// Get 2 contiguous 16bit from a Single and pack them into a 32bit
		/// </summary>
		/// <param name="value"></param>
		/// <param name="dword"></param>
		/// <param name="endian">target endianness</param>
		public static void ToBinary(float value, out uint dword, Endianness endian = Endianness.native)
		{
			bool needs_swap = !endian.IsPlatform();

			var bytes = BitConverter.GetBytes(value);
			dword = BitConverter.ToUInt32(bytes, 0);

			if (needs_swap) dword = dword << 16 | dword >> 16;
        }

        /// <summary>
        /// Get a Single from 2 contiguous 16bit
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="endian">the source endianness</param>
        /// <returns></returns>
        public static float FloatFromBinary(ArraySegment<ushort> bin, Endianness endian = Endianness.native)
		{
			if (bin.Count != 2)
				throw new ArgumentException(
					$"Unable to unmarshall binary to float. Expecting a source buffer of size 2, but was {bin.Count}");

			bool needs_swap = !endian.IsPlatform();
			int lsw = bin.Offset + (needs_swap ? 1 : 0);
			int msw = bin.Offset + (needs_swap ? 0 : 1);

			uint data32 = bin.Array[lsw] | (uint)bin.Array[msw] << 16;
			return BitConverter.ToSingle(BitConverter.GetBytes(data32), 0);
		}
		/// <summary>
		/// Get a Single from a 32bit integer
		/// </summary>
		/// <param name="bin"></param>
		/// <param name="endian">the source endianness</param>
		/// <returns></returns>
		public static float FloatFromBinary(uint bin, Endianness endian = Endianness.native)
        {
			if (!endian.IsPlatform()) bin = (bin << 16) | (bin >> 16);

			return BitConverter.ToSingle(BitConverter.GetBytes(bin), 0);
        }
    }
}
