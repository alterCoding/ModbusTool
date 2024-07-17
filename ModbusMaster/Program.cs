using System;
using System.Windows.Forms;
using Modbus.Common;

namespace ModbusMaster
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _ = GlobalExceptionHandling._once;

            var options = AppOptions.FromCommandLine();
            Application.Run(new MasterForm(options));
        }
    }
}
