using System;
using System.IO;
using System.Windows.Forms;

namespace GameOptimizer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Set exception handlers
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => LogException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => LogException(e.ExceptionObject as Exception);

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GameOptimizer.GameOptimizerForm());
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        static void LogException(Exception ex)
        {
            if (ex == null) return;
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                string logEntry = string.Format("[{0}] Error: {1}{2}{3}{2}{4}{2}", 
                    DateTime.Now, 
                    ex.Message, 
                    Environment.NewLine, 
                    ex.StackTrace, 
                    new string('-', 50));

                File.AppendAllText(logPath, logEntry);

                MessageBox.Show("The application encountered an unexpected error and needs to close.\n\nLog saved to: " + logPath, 
                    "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Silently fail if we can't write the log
            }
        }
    }
}
