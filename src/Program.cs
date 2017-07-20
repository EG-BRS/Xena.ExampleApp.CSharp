using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ExampleProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Test Client";

            

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5002")  //TODO what about this?!
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
