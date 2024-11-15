using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StarfallAfterlife.Launcher.Services
{
    public class FileLogger : IDisposable
    {
        public string LogDirectory => _directoryPath;

        private readonly string _directoryPath;
        private readonly int _maxFileCount = 10;
        private readonly int _bufferSize;
        private readonly long _maxFileSize;
        private readonly float _flushInterval;
        private readonly Timer _flushTimer;
        private StringBuilder _buffer;
        private string _currentLogFilePath;

        public FileLogger(string directoryPath, int bufferSize = 1000, long maxFileSize = 16777216, int maxFileCount = 32, float flushInterval = 10)
        {
            _directoryPath = directoryPath;
            _bufferSize = bufferSize;
            _maxFileSize = maxFileSize;
            _maxFileCount = maxFileCount;
            _flushInterval = flushInterval;
            _buffer = new StringBuilder(bufferSize);
            _flushTimer = new Timer(TimeSpan.FromSeconds(flushInterval));
            _flushTimer.Elapsed += (sender, e) => FlushBuffer();
            _flushTimer.Start();
            CreateNewLogFile();
        }

        public void Log(string message)
        {
            lock (_buffer)
            {
                _buffer.Append(message);

                if (_buffer.Length >= _bufferSize)
                    FlushBuffer();
            }
        }

        private void FlushBuffer()
        {
            lock (_buffer)
            {
                if (_buffer.Length == 0)
                    return;

                try
                {
                    File.AppendAllText(_currentLogFilePath, _buffer.ToString());
                    _buffer.Clear();

                    if (new FileInfo(_currentLogFilePath).Length >= _maxFileSize)
                        CreateNewLogFile();
                }
                catch { }
            }
        }

        private void CreateNewLogFile()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                _currentLogFilePath = Path.Combine(_directoryPath, $"log_{timestamp}.txt");

                Directory.CreateDirectory(_directoryPath);
                CleanupOldLogs();
            }
            catch { }
        }

        private void CleanupOldLogs()
        {
            try
            {
                var logFiles = Directory.GetFiles(_directoryPath, "log_*.txt");
                Array.Sort(logFiles);

                while (logFiles.Length > _maxFileCount)
                {
                    File.Delete(logFiles[0]);
                    logFiles = logFiles[1..];
                }
            }
            catch { }
        }

        public void Dispose()
        {
            _flushTimer.Stop();
            FlushBuffer();
        }
    }
}
