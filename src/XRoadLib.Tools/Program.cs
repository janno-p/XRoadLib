using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRoadLib.Tools.CodeGen;
using XRoadLib.Tools.Logging;

namespace XRoadLib.Tools
{
    public class Program
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;

        public Program()
        {
            loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new CommandOutputProvider());
            logger = loggerFactory.CreateLogger<ILogger<Program>>();
        }

        private int Run(string[] args)
        {
            try
            {
                var app = new CommandLineApplication();
                app.Name = "dotnet-xroad-gen";
                app.Description = "XRoadLib code generator";
                app.ShortVersionGetter = () => GetInformationalVersion();

                app.HelpOption("-?|-h|--help");

                var optVerbose = app.Option("-v|--verbose", "Verbose output", CommandOptionType.NoValue);
                var optCode = app.Option("-c|--code", "Generate code", CommandOptionType.NoValue);
                var optOutput = app.Option("-o|--output", "Output path", CommandOptionType.SingleValue);
                var optMapping = app.Option("-m|--mapping", "Customization mappings for generated code", CommandOptionType.SingleValue);

                var wsdlArgument = app.Argument("[wsdl]", "Url of service description file");

                app.OnExecute(async () =>
                {
                    var serviceProvider = ConfigureServices(opt =>
                    {
                        opt.WsdlUri = wsdlArgument.Value;
                        opt.IsCodeOutput = optCode.HasValue();
                        opt.IsVerbose = optVerbose.HasValue();
                        opt.MappingPath = optMapping.HasValue() ? new FileInfo(optMapping.Value()) : null;
                        opt.OutputPath = new DirectoryInfo(optOutput.HasValue() ? optOutput.Value() : "Output");
                    });

                    if (string.IsNullOrEmpty(wsdlArgument.Value))
                        throw new ArgumentNullException("WSDL location url is required.");

                    var generator = serviceProvider.GetRequiredService<IGenerator>();
                    await generator.GenerateAsync();

                    return 0;
                });

                return app.Execute(args);
            }
            catch (Exception exception)
            {
                logger.LogCritical($"Command failed: {exception}");
                return 1;
            }
        }

        private IServiceProvider ConfigureServices(Action<GeneratorOptions> optionsAction)
        {
            var services = new ServiceCollection();

            services.AddOptions();

            services.Configure<GeneratorOptions>(optionsAction);

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<IGenerator, Generator>();
            services.AddSingleton<Program>();

            return services.BuildServiceProvider();
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
