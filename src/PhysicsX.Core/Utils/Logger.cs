using System;
using System.IO;
using System.Text;

namespace PhysicsX.Core.Utils;

/// <summary>
/// 日志级别
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

/// <summary>
/// 简单的日志记录器
/// </summary>
public class Logger
{
    private static Logger? _instance;
    private static readonly object _lock = new object();

    private string _logFilePath;
    private LogLevel _minLevel;
    private bool _writeToConsole;
    private bool _writeToFile;
    private readonly object _fileLock = new object();

    private Logger()
    {
        _minLevel = LogLevel.Debug;
        _writeToConsole = true;
        _writeToFile = true;

        // 默认日志文件路径
        var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _logFilePath = Path.Combine(logsDir, $"physicsx_{timestamp}.log");
    }

    public static Logger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 配置日志器
    /// </summary>
    public void Configure(LogLevel minLevel, bool writeToConsole = true, bool writeToFile = true, string? customLogPath = null)
    {
        _minLevel = minLevel;
        _writeToConsole = writeToConsole;
        _writeToFile = writeToFile;

        if (!string.IsNullOrEmpty(customLogPath))
        {
            _logFilePath = customLogPath;
        }
    }

    /// <summary>
    /// 记录调试信息
    /// </summary>
    public void Debug(string message, string? category = null)
    {
        Log(LogLevel.Debug, message, category);
    }

    /// <summary>
    /// 记录一般信息
    /// </summary>
    public void Info(string message, string? category = null)
    {
        Log(LogLevel.Info, message, category);
    }

    /// <summary>
    /// 记录警告
    /// </summary>
    public void Warning(string message, string? category = null)
    {
        Log(LogLevel.Warning, message, category);
    }

    /// <summary>
    /// 记录错误
    /// </summary>
    public void Error(string message, Exception? exception = null, string? category = null)
    {
        var fullMessage = exception != null
            ? $"{message}\n{exception}"
            : message;
        Log(LogLevel.Error, fullMessage, category);
    }

    /// <summary>
    /// 记录致命错误
    /// </summary>
    public void Fatal(string message, Exception? exception = null, string? category = null)
    {
        var fullMessage = exception != null
            ? $"{message}\n{exception}"
            : message;
        Log(LogLevel.Fatal, fullMessage, category);
    }

    private void Log(LogLevel level, string message, string? category)
    {
        if (level < _minLevel)
            return;

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = level.ToString().ToUpper().PadRight(7);
        var categoryStr = string.IsNullOrEmpty(category) ? "" : $"[{category}] ";
        var logMessage = $"[{timestamp}] {levelStr} {categoryStr}{message}";

        // 控制台输出（带颜色）
        if (_writeToConsole)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = GetConsoleColor(level);
            Console.WriteLine(logMessage);
            Console.ForegroundColor = originalColor;
        }

        // 文件输出
        if (_writeToFile)
        {
            WriteToFile(logMessage);
        }
    }

    private void WriteToFile(string message)
    {
        try
        {
            lock (_fileLock)
            {
                File.AppendAllText(_logFilePath, message + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            // 写入文件失败，只输出到控制台
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }

    private ConsoleColor GetConsoleColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => ConsoleColor.White
        };
    }

    /// <summary>
    /// 获取当前日志文件路径
    /// </summary>
    public string GetLogFilePath() => _logFilePath;

    /// <summary>
    /// 清空旧日志文件（保留最近N天）
    /// </summary>
    public void CleanOldLogs(int daysToKeep = 7)
    {
        try
        {
            var logsDir = Path.GetDirectoryName(_logFilePath);
            if (string.IsNullOrEmpty(logsDir) || !Directory.Exists(logsDir))
                return;

            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var files = Directory.GetFiles(logsDir, "physicsx_*.log");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                    Info($"Deleted old log file: {fileInfo.Name}", "Logger");
                }
            }
        }
        catch (Exception ex)
        {
            Error("Failed to clean old logs", ex, "Logger");
        }
    }
}
