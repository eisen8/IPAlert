using Serilog;
using System.Diagnostics;

namespace IPAlert
{
    /// <summary>
    /// Basic logger class using SeriLog
    /// </summary>
    public class Logger
    {
        public Logger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("./logs/ip_alert_logs.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7) // Keep only the last 7 log files
                .CreateLogger();
        }

        public void Info(string message)
        {
            Log.Information(message);
            if (Debugger.IsAttached)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                System.Diagnostics.Debug.WriteLine($"{time} - INFO - {message}");
            }
        }

        public void Debug(string message)
        {
            Log.Debug(message);
            if (Debugger.IsAttached)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                System.Diagnostics.Debug.WriteLine($"{time} - DEBUG - {message}");
            }
        }

        public void Error(string message, Exception ex)
        {
            Log.Error(ex, message);
            if (Debugger.IsAttached)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                System.Diagnostics.Debug.WriteLine($"{time} - ERROR - {message}");
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        public void Error(string message)
        {
            Log.Error(message);
            if (Debugger.IsAttached)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                System.Diagnostics.Debug.WriteLine($"{time} - ERROR - {message}");
            }
        }
    }
}
