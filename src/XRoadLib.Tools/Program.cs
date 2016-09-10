using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using XRoadLib.Tools.CodeGen;
using XRoadLib.Tools.CodeGen.CodeFragments;
using XRoadLib.Tools.CodeGen.Extensions;

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
                    if (string.IsNullOrEmpty(wsdlArgument.Value))
                        throw new ArgumentNullException("WSDL location url is required.");

                    var doc = await LoadServiceDescriptionAsync(wsdlArgument.Value);

                    var directory = new DirectoryInfo(optOutput.HasValue() ? optOutput.Value() : "Output");
                    if (!directory.Exists)
                        directory.Create();

                    new Generator(doc, directory).Generate();

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

        private async Task<XDocument> LoadServiceDescriptionAsync(string uri)
        {
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                logger.LogInformation($"Requesting service description from network location: {uri}.");
                using (var client = new HttpClient())
                using (var stream = await client.GetStreamAsync(uri))
                    return XDocument.Load(stream);
            }

            var fileInfo = new FileInfo(uri);
            if (fileInfo.Exists)
            {
                logger.LogInformation($"Requesting service description from file: {fileInfo.FullName}.");
                return XDocument.Load(fileInfo.FullName);
            }

            throw new FileNotFoundException($"Cannot resolve wsdl location `{uri}`.");
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
