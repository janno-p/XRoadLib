using System.Reflection;
using Calculator.Contract;
using Calculator.Handler;
using Calculator.WebService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRoadLib;
using XRoadLib.Extensions;
using XRoadLib.Schema;

namespace Calculator
{
    public class Startup
    {
        private readonly IXRoadProtocol protocol = new XRoadProtocol("3.1", new SchemaExporterXRoad31("producer", typeof(Startup).GetTypeInfo().Assembly));

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISumOfIntegers, SumOfIntegersWebService>();
            services.AddSingleton(provider => new CalculatorHandler(provider, new[] { protocol }, null));
        }

        public void Configure(IApplicationBuilder application, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            application.UseXRoadLib(options =>
            {
                options.AddRequestHandler<CalculatorHandler>();
                options.SupportedProtocols.Add(protocol);
            });
        }
    }
}