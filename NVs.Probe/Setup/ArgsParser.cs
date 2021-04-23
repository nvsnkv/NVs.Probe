using System.Collections.Generic;
using NVs.Probe.Metrics;
using CommandLine;
using System;
using System.Linq;

namespace NVs.Probe.Setup
{
    sealed class ArgsParser 
    {
        private readonly Parser parser;
        private readonly string optionsDelimiter = "--";

        public ArgsParser()
        {
            parser = new Parser((c) => c.EnableDashDash = true);
        }

        public (Options, IEnumerable<MetricConfig>) Parse(IReadOnlyList<string> args) 
        {
            var parserResult = parser.ParseArguments<Options>(args);
            var options = parserResult.MapResult(
                (Options o) => o,
                (IEnumerable<Error> errs) => throw new ArgumentException("Failed to parse arguments!")
                {
                    Data = { {"Errors", errs} }
                }
            );

            var configs = new MetricConfigBuilder(args.SkipWhile(a => a != optionsDelimiter).Skip(1)).Build();

            return (options, configs);
        }
    }
}