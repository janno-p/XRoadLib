using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools
{
    public class Program
    {
        private readonly ILogger logger;

        private Program()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new CommandOutputProvider());
            logger = loggerFactory.CreateLogger<Program>();
        }

        private int Run(string[] args)
        {
            try
            {
                var cu = SF.CompilationUnit()
                        .AddUsings(SF.UsingDirective(SF.IdentifierName("System")))
                        .AddUsings(SF.UsingDirective(SF.IdentifierName("System.Generic")));

                var app = new CommandLineApplication();
                app.Name = "dotnet-xroad-gen";
                app.Description = "XRoadLib code generator";
                app.ShortVersionGetter = () => GetInformationalVersion();

                app.HelpOption("-?|-h|--help");

                var verboseOption = app.Option("-v|--verbose", "Verbose output", CommandOptionType.NoValue);

                app.OnExecute(() =>
                {
                    logger.LogInformation("test");
                    return 0;
                });

                return app.Execute(args);
            }
            catch (Exception exception)
            {
                logger.LogCritical("Command failed", exception.Message);
                return 1;
            }
        }

        private static string GetInformationalVersion()
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attribute == null ? assembly.GetName().Version.ToString() : attribute.InformationalVersion;
        }

        public static int Main(string[] args)
        {
            return new Program().Run(args);
        }
    }
}
