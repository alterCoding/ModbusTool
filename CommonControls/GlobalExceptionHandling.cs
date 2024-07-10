using System;
using System.Threading;
using System.Windows.Forms;

namespace Modbus.Common
{
    /// <summary>
    /// Enable a simple message box for unhandled exceptions at main thread level and worker threads level
    /// </summary>
    public class GlobalExceptionHandling
    {
        /// <summary>
        /// do it once
        /// </summary>
        private GlobalExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.ThreadException += OnUnhandledException;

        }
        /// <summary>
        /// Global exceptions in Non User Interface (other or worker threads) handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowException(e.ExceptionObject as Exception);
        }
        
        /// <summary>
        /// Global exceptions in main thread (GUI)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            ShowException(e.Exception);
        }

        private static void ShowException(Exception e)
        {
            string message = e.InnerException?.Message;
            if (message != null) message = string.Concat(e.Message, Environment.NewLine, "<-- ", message);
            else message = e.Message;

            MessageBox.Show(message, e.TargetSite.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static readonly GlobalExceptionHandling _once = new GlobalExceptionHandling();
    }
}
