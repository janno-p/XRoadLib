using System.Reflection;
using Calculator.Contract;
using Calculator.Handler;
using Calculator.WebService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRoadLib.Extensions;
using XRoadLib.Protocols;

namespace Calculator
{
    public class Startup
    {
        private readonly XRoadProtocol protocol;

        public Startup()
        {
            protocol = new XRoad31Protocol("calculator", "http://calculator.x-road.eu/producer/");
            protocol.SetContractAssembly(typeof(Startup).GetTypeInfo().Assembly, null);
        }

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