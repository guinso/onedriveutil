using Microsoft.Extensions.Logging;

namespace MsGraph.Core.FileLogger
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private string _logDirectory;
        public FileLoggerProvider(string logDirectory)
        {
            _logDirectory = logDirectory;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SimpleFileLogger(categoryName, _logDirectory);
        }

        public void Dispose()
        {
            
        }
    }
}
