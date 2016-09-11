using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace XRoadLib.Tools.Logging
{
    public class CommandOutputProvider : ILoggerProvider
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public ILogger CreateLogger(string name)
        {
            return new CommandOutputLogger(this, RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        }

        public void Dispose()
        { }
    }
}