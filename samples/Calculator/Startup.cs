using System.Diagnostics.CodeAnalysis;
using Calculator.Contract;
using Calculator.WebService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XRoadLib.Extensions.AspNetCore;

namespace Calculator
{
    public class Startup
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMemberAsStatic")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddXRoadLib();

            services.AddSingleton<ICalculate, CalculateWebService>();
            services.AddSingleton<ISumOfIntegers, SumOfIntegersWebService>();
            services.AddSingleton<CalculatorServiceManager>();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMemberAsStatic")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", c => c.ExecuteWsdlRequest<CalculatorServiceManager>());
                endpoints.MapPost("/", c => c.ExecuteWebServiceRequest<CalculatorServiceManager>());
            });
        }
    }
}