using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Versioning.Example.Api.Test
{
    public class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly Func<ITestOutputHelper?> _outputHelperFn;

        public CapturingLoggerProvider(Func<ITestOutputHelper?> outputHelperFn)
        {
            _outputHelperFn = outputHelperFn ?? throw new ArgumentNullException(nameof(outputHelperFn));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CapturingLogger(_outputHelperFn);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class CapturingLogger : ILogger
    {
        private readonly Func<ITestOutputHelper?> _outputHelperFn;

        public CapturingLogger(Func<ITestOutputHelper?> outputHelperFn)
        {
            _outputHelperFn = outputHelperFn;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return new TestLoggingScope();
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, System.Exception? exception, System.Func<TState, System.Exception?, string> formatter)
        {
            var logMessage = formatter(state, exception);
            var outputHelper = _outputHelperFn() ?? throw new InvalidOperationException("Null output helper");
            outputHelper.WriteLine($"{logLevel} - {logMessage}");
        }

        internal sealed class TestLoggingScope : IDisposable
        {
            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
