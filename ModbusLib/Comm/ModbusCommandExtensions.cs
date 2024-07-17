using System;
using System.Collections.Generic;

namespace ModbusLib
{
    using Protocols;

    public static class ModbusCommandExtensions
    {
        public static bool IsDataRead(this ModbusCommand cmd)
        {
            return cmd.FunctionCode >= 1 && cmd.FunctionCode <= 4;
        }
        public static bool IsDataWrite(this ModbusCommand cmd)
        {
            return cmd.FunctionCode == 5 || cmd.FunctionCode == 6 || cmd.FunctionCode == 15 || cmd.FunctionCode == 16;
        }

        /// <summary>
        /// Informal text representation of a <see cref="ModbusCommand"/> object according to the pattern:<br/>
        /// "#{transacID} Func({code}) [{(R)ead(W)rite(D)iagnostics}] {offset}:{count} [except:{exception.code}]" <br/>
        /// e.g "#0 func(1) [R] 0x100:2" when successfully reading two coils at address 0x100  (or unprocessed) <br/>
        /// or  "#1 func(1) [R] 0x100:2 raised:illegalDataAddress" when reading invalid address
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>empty if invalid function code</returns>
        public static string Caption(this ModbusCommand cmd)
        {
            string label = string.Empty;
            int fcode = cmd.FunctionCode;
            if (fcode > 0 && fcode <= 7) label = _cmdCaptions[fcode + 1].Value;

            var key = new KeyValuePair<byte, string>((byte)fcode, string.Empty);
            int index = Array.BinarySearch(_cmdCaptions, key, FunctionCodeLookup.Instance);

            if (index != -1) label = _cmdCaptions[index].Value;

            if (label != string.Empty)
            {
                if (cmd.ExceptionCode == 0)
                    return string.Concat($"#{cmd.TransId} ", label, $" 0x{cmd.Offset:X4}:{cmd.Count}");
                else
                    return string.Concat($"#{cmd.TransId} ", label, $" 0x{cmd.Offset:X4}:{cmd.Count} raised:",
                        ModbusException.ToExceptionCode(cmd.ExceptionCode).ToString());
            }

            return string.Empty;
        }

        private static KeyValuePair<byte, string> makeFunction(byte code, string caption) =>
                new KeyValuePair<byte, string>(code, caption);

        /// <summary>
        /// Informal label for the supported modbus functions 
        /// </summary>
        private static KeyValuePair<byte, string>[] _cmdCaptions = new []
        {
            makeFunction(1, "func(1) [R]"),  //read coils
            makeFunction(2, "func(2) [R]"),  //read di
            makeFunction(3, "func(3) [R]"),  //read hr
            makeFunction(4, "func(4) [R]"),  //read ir
            makeFunction(5, "func(5) [W]"),  //write single coil
            makeFunction(6, "func(6) [W]"),  //write single reg 
            makeFunction(7, "func(7) [D]"),  //diag read except
            makeFunction(15, "func(15) [W]"),  //write coils
            makeFunction(16, "func(16) [W]"),  //write multi reg
        };

        private class FunctionCodeLookup : IComparer<KeyValuePair<byte, string>>
        {
            public int Compare(KeyValuePair<byte, string> x, KeyValuePair<byte, string> y)
            {
                return x.Key < y.Key ? -1 : x.Key > y.Key ? +1 : 0;
            }

            public static readonly FunctionCodeLookup Instance = new FunctionCodeLookup();
        }
    }
}
