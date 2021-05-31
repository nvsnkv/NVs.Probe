using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CommandLine;
using NVs.Probe.Configuration;

namespace NVs.Probe.Client
{
    internal sealed class Bootstrapper
    {
        private readonly Parser argsParser;
        private readonly IConsole console;
        
        public Bootstrapper(Parser argsParser, IConsole console)
        {
            this.argsParser = argsParser;
            this.console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void Start(string id, string configPath)
        {
            if (string.IsNullOrEmpty(id))
            {
                console.WriteError("Unable to deploy new instance of probe: no id provided!");
                throw new ArgumentNullException(nameof(id));
            }

            if (!File.Exists(configPath))
            {
                console.WriteError("Unable to deploy new instance of probe: configuration file does not exists, or inaccessible!");
                throw new FileNotFoundException("Configuration file not found!", configPath);
            }

            var path = GetExecutablePath();
            var args = argsParser.FormatCommandLine(new ServeArguments(configPath, id));

            var info = path.EndsWith(".dll") 
                ? new ProcessStartInfo("dotnet", $"\"{path}\" {args}")
                : new ProcessStartInfo(path, args);

            var process = Process.Start(info);
            if (process == null)
            {
                console.WriteError("Unable to deploy new instance of probe: failed to start process!");
                throw new InvalidOperationException("Process instance was not created!");
            }
            
            console.WriteLine(id);
        }

        private static string GetExecutablePath()
        {
            var assembly = Assembly.GetCallingAssembly();
            if (assembly?.CodeBase == null)
            {
                throw new InvalidOperationException("Failed to identify calling assembly!");
            }

            var path = new Uri(assembly.CodeBase).LocalPath;
            return path;
        }

        public void Stop(string id)
        {
            throw new NotImplementedException();
        }
    }
}