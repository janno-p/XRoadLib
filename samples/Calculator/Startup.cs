using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XRoadLib.Extensions.AspNetCore;

namespace Calculator
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(Startup));

            services.AddXRoadLib(sp => request => sp.GetRequiredService<IMediator>().Send(request, default));

            services.AddSingleton<CalculatorServiceManager>();
        }

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