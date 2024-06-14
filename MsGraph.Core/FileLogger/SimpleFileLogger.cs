using Microsoft.Extensions.Logging;

namespace MsGraph.Core.FileLogger
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line
    /// https://stackoverflow.com/questions/40073743/how-to-log-to-a-file-without-using-third-party-logger-in-net-core
    /// </summary>
    public class SimpleFileLogger : ILogger
    {
        private string _logFilePath;
        private string _categoryName;
        private static object _locker = new object();

        public SimpleFileLogger(string categoryName, string LoggingDirectory)
        {
            _categoryName = categoryName;

            if(Directory.Exists(LoggingDirectory) == false)
            {
                throw new DirectoryNotFoundException(LoggingDirectory);
            }
            else
            {
                var finalLogDir = Path.Combine(LoggingDirectory, "logs");
                Directory.CreateDirectory(finalLogDir);
                _logFilePath = Path.Combine(finalLogDir, "log " + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            }                
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if(formatter is not null)
            {
                lock(_locker)
                {
                    string categoryName = string.IsNullOrEmpty(_categoryName) ? string.Empty : string.Format("[{0}]", _categoryName);
                    string msg = string.Format("[{0}][{1}]{2} {3}", 
                        DateTime.Now, 
                        logLevel,
                        categoryName,
                        formatter(state, exception));

                    using (var writter = new StreamWriter(_logFilePath, true))
                    {
                        writter.WriteLine(msg);
                        if(exception is not null)
                        {
                            writter.WriteLine($"error message: {exception.Message}");
                            writter.WriteLine($"stack trace:");
                            writter.WriteLine(exception.StackTrace);
                        }

                        writter.Flush();
                    }
                }
            }
        }
    }
}
