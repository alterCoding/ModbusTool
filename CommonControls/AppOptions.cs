using System;
using System.Diagnostics;

namespace Modbus.Common
{
    /// <summary>
    /// Application options. Those parameters are especially provided with the command line
    /// </summary>
    public class AppOptions
    {
        /// <summary>
        /// Path of the xml file to be loaded (if any) to get the modbus communication parameters of the endpoint <br/>
        /// If application is a master device, the endpoint is the modbus slave to be scanned (reminder: the project is
        /// single device capable, whereas a real scanner is expected to target multi-devices)<br/>
        /// If application is a slave, the endpoint is the modbus device endpoint to be (locally) executed
        /// </summary>
        public string EndPointFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Start communication immediately.<br/>
        /// (The master starts the target device slave scanning or the device slave starts listening for requests)
        /// </summary>
        /// <remarks>
        /// May not be a good idea if not used both with <see cref="EndPointFilePath"/> as the last device parameters
        /// are loaded by default (which might be unexpected)
        /// </remarks>
        public bool AutoStart { get; private set; } = false;

        /// <summary>
        /// Path of the csv file (if any) from which the data table can be retrieved.
        /// </summary>
        public string DataTableFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Modbus registers addresses are formatted in hexa by default
        /// </summary>
        public bool? AddrFormatDefaultIsHexa { get; private set; }

        public static AppOptions FromCommandLine()
        {
            var options = new AppOptions();

            var args = CommandLineParsing.GetArguments();

            foreach (var a in args)
            {
                if (a.Key == _endpointParameter)
                    options.EndPointFilePath = a.Value;
                else if (a.Key == _autoStart)
                    options.AutoStart = string.IsNullOrWhiteSpace(a.Value) || Convert.ToBoolean(a.Value);
                else if (a.Key == _dataTableParameter)
                    options.DataTableFilePath = a.Value;
                else if (a.Key == _addrFormatHex)
                    options.AddrFormatDefaultIsHexa =string.IsNullOrWhiteSpace(a.Value) || Convert.ToBoolean(a.Value);
                else
                    Debug.WriteLine($"'{a.Key}' is not a command line expected argument");
            }

            return options;
        }

        public static readonly string _endpointParameter = "with-device";
        public static readonly string _dataTableParameter = "with-datatable";
        public static readonly string _autoStart = "autostart";
        public static readonly string _addrFormatHex = "hexAddr";
    }
}
