using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NVs.Probe.Experiments;
using Serilog;
using Serilog.Events;

namespace NVs.Probe
{
    class Program
    {
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                Log.Information("Starting host..");
                BuildHost(args).Run();
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly!");
                return -1;
            }
        }

        private static IHost BuildHost(string[] args)
        {
            return new HostBuilder()
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IHostedService>(new Payload());
                    s.AddSingleton<IExperimenter>();
                })
                .UseSerilog()
                .Build();
        }
    }
}
