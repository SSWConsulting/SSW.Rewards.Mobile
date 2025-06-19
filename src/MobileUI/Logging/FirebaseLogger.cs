using Microsoft.Extensions.Logging;
using Plugin.Firebase.Crashlytics;

namespace SSW.Rewards.Mobile.Logging;

public class FirebaseLogger : ILogger
{
    private readonly string _categoryName;

    public FirebaseLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logMessage = $"[{_categoryName}] {message}";

        // Log to Firebase Crashlytics
        CrossFirebaseCrashlytics.Current.Log(logMessage);

        // For errors and critical, also record the exception
        if (exception != null && logLevel >= LogLevel.Error)
        {
            CrossFirebaseCrashlytics.Current.RecordException(exception);
        }

        // Also log to console for debugging
        Console.WriteLine($"[{logLevel}] {logMessage}");
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}

public class FirebaseLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new FirebaseLogger(categoryName);

    public void Dispose() { }
}