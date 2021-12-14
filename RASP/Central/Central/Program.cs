using DataCommunicator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Central
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(op =>
                    {
                        op.Listen(System.Net.IPAddress.Any, 80);
                        op.Listen(System.Net.IPAddress.Any, 443, listenOp =>
                        {
                            listenOp.UseHttps("localhost.pfx", "password");
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
