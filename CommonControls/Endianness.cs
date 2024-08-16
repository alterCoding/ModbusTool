using System;

namespace Modbus.Common.BCL
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

		public static Endianness Swapped(this Endianness endian)
        {
			if (endian == Endianness.undefined)
				throw new InvalidOperationException("Unable to stand for an undefined endianness");

			if (endian == Endianness.BE) return Endianness.LE;
			else if (endian == Endianness.LE) return Endianness.BE;
			else throw new NotImplementedException($"how to swap {endian} ?");
        }
	}
}
