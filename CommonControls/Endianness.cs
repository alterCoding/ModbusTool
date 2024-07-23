using System;

namespace Modbus.Common
{
	/// <summary>
	/// Basic endianness
	/// </summary>
	/// <remarks>
	/// We only cope with common endiannesses, but future work should rather see the topic as a complete set of bytes
	/// and words manipulation (as devices implementations are not homogeneous). Integers and floating point endianness
	/// may be different, the modbus base is not always big-endian, manufacturers don't see the same thing ... and 
	/// exotic implementations are common <br/>
	/// As of now, we only support little endian platform for the local executable environment
	/// </remarks>
	/// @todo implement and test for linux env
	public enum Endianness
	{
		undefined = 0,
		native,
		BE,
		LE	
	}

	public static class Platform
	{
		/// <summary>
		/// Get the local endianness
		/// </summary>
		public static readonly Endianness Endianness = BitConverter.IsLittleEndian ? Endianness.LE : Endianness.BE;
	}

	public static class EndiannessExtensions
	{
		/// <summary>
		/// Enforce <paramref name="endian"/> is little or big endian
		/// </summary>
		/// <param name="endian"></param>
		/// <returns></returns>
		public static Endianness ShouldBeDefined(this Endianness endian)
		{
			return endian <= Endianness.native ? Platform.Endianness : endian;
		}

		public static bool IsPlatform(this Endianness endian)
		{
			return endian <= Endianness.native || endian == Platform.Endianness;
		}
	}

	/// <summary>
	/// Marshalling operations <br/>
	/// Scope: we only cope with words (16bit) endianness topic. The byte-level is related to the low level area, which
	/// is managed in the communication library (i.e not here)
	/// </summary>
	/// @todo When the project will be ready to be ported on .net instead of framework, rewrite methods with the new 
	/// span/memory/unsafe/marshalling features instead of BitConverter
	public static class Marshaller
	{
		/// <summary>
		/// </summary>
		/// <param name="value"></param>
		/// <param name="bin"></param>
		/// <param name="endian">the target endianness</param>
		public static void ToBinary(float value, ArraySegment<ushort> bin, Endianness endian = Endianness.undefined)
		{
			if (bin.Count != 2)
				throw new ArgumentException(
					$"Unable to marshall float to binary. Expecting a target buffer of size 2, but was {bin.Count}");

			bool needs_swap = !endian.IsPlatform();
			int lsw = bin.Offset + (needs_swap ? 1 : 0);
			int msw = bin.Offset + (needs_swap ? 0 : 1);

			var bytes = BitConverter.GetBytes(value);

			bin.Array[lsw] = (ushort)(bytes[0] | bytes[1] << 8);
			bin.Array[msw] = (ushort)(bytes[2] | bytes[3] << 8);
		}

		/// <summary>
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
	}
}
