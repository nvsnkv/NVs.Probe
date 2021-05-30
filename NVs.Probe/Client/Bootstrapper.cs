using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using NVs.Probe.Configuration;
using Serilog;

namespace NVs.Probe.Client
{
    internal sealed class Bootstrapper
    {
        private readonly Parser parser;
        private ILogger logger;
        private readonly Action<string> writeLine;

        public Bootstrapper(Parser parser, ILogger logger, Action<string> writeLine)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.writeLine = writeLine ?? throw new ArgumentNullException(nameof(writeLine));
        }

        public void Start(string id, string configPath)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (configPath == null) throw new ArgumentNullException(nameof(configPath));

            logger.Debug("Building command line arguments...");
            var args = parser.FormatCommandLine(new ServeArguments(configPath, id));
            logger.Debug("Done!");

            logger.Debug("Creating the process...");
            var assembly = Assembly.GetCallingAssembly();
            if (assembly?.CodeBase == null)
            {
                throw new InvalidOperationException("Failed to identify calling assembly!");
            }

            var path = new Uri(assembly.CodeBase).LocalPath;
            var info = path.EndsWith(".dll") 
                ? new ProcessStartInfo("dotnet", $"\"{path}\" {args}")
                : new ProcessStartInfo(path, args);

            logger.Debug("Attempting to start {@executable} with following args {@args}...", info.FileName, info.Arguments);
            var process = Process.Start(info);
            if (process == null)
            {
                throw new InvalidOperationException("Process instance was not created!");
            }

            logger.Information("Started process {@pid} for instance {@id}", process.Id, id);
            writeLine(id);
        }
    }
}