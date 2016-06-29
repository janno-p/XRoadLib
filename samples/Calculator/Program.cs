using Microsoft.AspNetCore.Hosting;

namespace Calculator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5000/")
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
