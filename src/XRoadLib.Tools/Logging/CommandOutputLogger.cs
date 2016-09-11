using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace XRoadLib.Tools.Logging
{
    public class CommandOutputLogger : ILogger
    {
        private readonly CommandOutputProvider provider;
        private readonly AnsiConsole console;

        public CommandOutputLogger(CommandOutputProvider provider, bool useConsoleColor)
        {
            this.provider = provider;
            console = AnsiConsole.GetOutput(useConsoleColor);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= provider.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
                console.WriteLine($"{Caption(logLevel)}: {formatter(state, exception)}");
        }

        private string Caption(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace: return "\x1b[35mtrace\x1b[39m";
                case LogLevel.Debug: return "\x1b[35mdebug\x1b[39m";
                case LogLevel.Information: return "\x1b[32minfo\x1b[39m";
                case LogLevel.Warning: return "\x1b[33mwarn\x1b[39m";
                case LogLevel.Error: return "\x1b[31mfail\x1b[39m";
                case LogLevel.Critical: return "\x1b[31mcritical\x1b[39m";
                default: throw new Exception("Unknown LogLevel");
            }
        }
    }
}