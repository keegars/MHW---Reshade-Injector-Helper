using System;
using System.IO;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class ErrorLogHelper
    {
        private static readonly object lockObject = new object();

        public static void Log(Exception ex)
        {
            Console.WriteLine(ex.Message);

            lock (lockObject)
            {
                // Log the error message and stack trace to a file
                File.AppendAllText(ErrorLogSingleton.Instance.ErrorLogFileInfo.FullName, $"{Environment.NewLine} {DateTime.Now:dd/MM/yyyy HH:mm:ss} {Environment.NewLine} {ex.Source} {ex.Message} -  {ex.InnerException} - {ex.StackTrace}");
            }

          
        }

        public static void Log(string message)
        {
            Console.WriteLine(message);

            lock (lockObject)
            {
                File.AppendAllText(ErrorLogSingleton.Instance.ErrorLogFileInfo.FullName, $"{Environment.NewLine} {DateTime.Now:dd/MM/yyyy HH:mm:ss} {Environment.NewLine} {message}");
            }
        }

        public static void Log(string message, Exception ex)
        {
            Log(message);
            Log(ex);
        }
    }

    internal sealed class ErrorLogSingleton
    {
        private static readonly Lazy<ErrorLogSingleton> lazy =
            new Lazy<ErrorLogSingleton>(() => new ErrorLogSingleton());

        private ErrorLogSingleton()
        {
            ErrorLogFileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt"));

            if (!ErrorLogFileInfo.Exists)
            {
                ErrorLogFileInfo.Create();
            }

            ErrorLogFileInfo.Refresh();
        }

        public static ErrorLogSingleton Instance
        { get { return lazy.Value; } }

        public FileInfo ErrorLogFileInfo { get; private set; }
    }
}