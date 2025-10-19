using System;
using System.IO;

namespace _4Chess
{
    public static class Logger
    {
        private static StreamWriter? _logWriter;
        private static readonly object _lock = new object();
        private static string _logFilePath = "";

        public static void Initialize()
        {
            try
            {
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logDirectory);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                _logFilePath = Path.Combine(logDirectory, $"4Chess_{timestamp}.log");

                _logWriter = new StreamWriter(_logFilePath, true)
                {
                    AutoFlush = true
                };

                Log("=".PadRight(80, '='));
                Log($"4Chess - Log gestartet: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Log("=".PadRight(80, '='));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Initialisieren des Loggers: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            lock (_lock)
            {
                string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";

                // In Konsole ausgeben
                Console.WriteLine(timestampedMessage);

                // In Datei schreiben
                try
                {
                    _logWriter?.WriteLine(timestampedMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Schreiben ins Log: {ex.Message}");
                }
            }
        }

        public static void LogError(string message, Exception? ex = null)
        {
            string errorMessage = ex != null
                ? $"ERROR: {message} - {ex.Message}\n{ex.StackTrace}"
                : $"ERROR: {message}";

            Log(errorMessage);
        }

        public static void LogWarning(string message)
        {
            Log($"WARNING: {message}");
        }

        public static void LogInfo(string message)
        {
            Log($"INFO: {message}");
        }

        public static void Close()
        {
            lock (_lock)
            {
                Log("=".PadRight(80, '='));
                Log($"4Chess - Log beendet: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Log("=".PadRight(80, '='));

                _logWriter?.Close();
                _logWriter?.Dispose();
                _logWriter = null;

                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    Console.WriteLine($"\nLog gespeichert in: {_logFilePath}");
                }
            }
        }
    }
}
