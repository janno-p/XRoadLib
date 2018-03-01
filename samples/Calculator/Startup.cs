using System.Reflection;
using Calculator.Contract;
using Calculator.WebService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using XRoadLib;
using XRoadLib.Extensions.AspNetCore;
using XRoadLib.Headers;
using XRoadLib.Schema;

namespace Calculator
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICalculate, CalculateWebService>();
            services.AddSingleton<ISumOfIntegers, SumOfIntegersWebService>();

            services.AddXRoadLib(opt =>
            {
                var serviceManager = new ServiceManager<XRoadHeader40>(
                    "4.0",
                    new DefaultSchemaExporter("http://calculator.x-road.eu/", typeof(Startup).GetTypeInfo().Assembly)
                );
                
                opt.AddRequestHandler("/", serviceManager)
                   .AddWsdlHandler("/", serviceManager);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseXRoadLib();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
