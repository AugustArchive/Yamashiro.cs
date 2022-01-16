using System;

namespace Yamashiro.Logging
{
    public enum LogLevel
    {
        Info,
        Warn,
        Error,
        Debug,
        Command
    }

    public class Logger
    {
        public string Namespace;

        public Logger(string ns)
        {
            Namespace = ns;
        }

        private void LogAsync(LogLevel level, string message, Exception ex = null)
        {
            var text = $"[{DateTime.Now.ToString("hh:mm:ss")}] [{level}] [{Namespace}] <=> {ex?.ToString() ?? message}";
            Console.Out.WriteLine(text);
        }

        public void Info(params string[] message) => LogAsync(LogLevel.Info, string.Join("\n", message));
        public void Warn(params string[] message) => LogAsync(LogLevel.Warn, string.Join("\n", message));
        public void Error(params string[] message) => LogAsync(LogLevel.Error, string.Join("\n", message));
        public void Error(Exception ex) => LogAsync(LogLevel.Error, "Unknown exception happened", ex: ex);
        public void Debug(params string[] message) => LogAsync(LogLevel.Debug, string.Join("\n", message));
    }
}