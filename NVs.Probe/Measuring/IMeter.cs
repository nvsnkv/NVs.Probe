using System.Threading;
using System.Threading.Tasks;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measuring
{
    internal interface IMeter {
        Task<Measurement> Measure(MetricConfig config, CancellationToken ct);
    }
}
