using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

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
                var optNamespace = app.Option("-n|--namespace", "Namespace root for generated code", CommandOptionType.SingleValue);

                var wsdlArgument = app.Argument("[wsdl]", "Url of service description file");

                app.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(wsdlArgument.Value))
                        throw new ArgumentNullException("WSDL location url is required.");

                    var fileLocation = ResolveUri(wsdlArgument.Value);
                    var doc = XDocument.Load(fileLocation);

                    var directory = new DirectoryInfo("Output");
                    if (!directory.Exists)
                        directory.Create();

                    var definitionsElement = doc.Element(XName.Get("definitions", NamespaceConstants.WSDL));

                    CodeGen.ServiceGenerator.GenerateServiceUnit(definitionsElement.Element(XName.Get("service", NamespaceConstants.WSDL)));

                    foreach (var bindingElement in definitionsElement.Elements(XName.Get("binding", NamespaceConstants.WSDL)))
                       new CodeGen.BindingGenerator(bindingElement).Generate();

                    logger.LogInformation(fileLocation);
                    return 0;
                });

                return app.Execute(args);
            }
            catch (Exception exception)
            {
                logger.LogCritical($"Command failed: {exception.Message}");
                return 1;
            }
        }

        private static string GetInformationalVersion()
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attribute == null ? assembly.GetName().Version.ToString() : attribute.InformationalVersion;
        }

        private static string ResolveUri(string uri)
        {
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                return uri;

            var fileInfo = new FileInfo(uri);
            if (fileInfo.Exists)
                return fileInfo.FullName;

            throw new FileNotFoundException($"Cannot resolve wsdl location `{uri}`.");
        }

        public static int Main(string[] args)
        {
            return new Program().Run(args);
        }
    }
}
