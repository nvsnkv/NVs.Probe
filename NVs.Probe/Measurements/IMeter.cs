using System.Threading;
using System.Threading.Tasks;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measurements
{
    interface IMeter {
        Task<Measurement> Measure(MetricConfig config, CancellationToken ct);
    }
}
