using System;

namespace Modbus.Common
{
    using BCL;

    public enum DisplayFormat
    {
        LED,
        Binary,
        Hex,
        Integer,
        Float32
    }

    public enum CommunicationMode
    {
        TCP,
        UDP,
        RTU
    }

    public enum Function
    {
        CoilStatus,
        InputStatus,
        HoldingRegister,
        InputRegister
    }

    public static class DisplayFormatExtensions
    {
        public static bool Is32Bit(this DisplayFormat fmt) => fmt == DisplayFormat.Float32;

        public static ArithmeticValueFormat ToValueFormat(this DisplayFormat fmt)
        {
            if (fmt == DisplayFormat.Hex) return ArithmeticValueFormat.hexa;
            else if (fmt == DisplayFormat.Integer || fmt == DisplayFormat.Float32) return ArithmeticValueFormat.@decimal;
            else if (fmt == DisplayFormat.Binary) return ArithmeticValueFormat.binary;
            else throw new NotImplementedException($"Don't know how to map {fmt} to {nameof(ArithmeticValueFormat)}");
        }
    }
}
