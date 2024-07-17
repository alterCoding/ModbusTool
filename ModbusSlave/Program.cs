using System;
using Modbus.Common;
using System.Windows.Forms;

namespace ModbusSlave
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _ = GlobalExceptionHandling._once;

            var options = AppOptions.FromCommandLine();
            Application.Run(new SlaveForm(options));
        }
    }
}
