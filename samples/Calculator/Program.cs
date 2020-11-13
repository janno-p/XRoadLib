using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Calculator
{
    public static class Program
    {
        public static void Main(string[] args) =>
            BuildWebHost(args).Run();

        private static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(web => web.UseStartup<Startup>())
                .Build();
    }
}
