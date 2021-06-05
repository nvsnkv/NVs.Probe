using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NVs.Probe.Measuring;
using NVs.Probe.Metrics;

namespace NVs.Probe.Mqtt 
{
    internal interface IMqttAdapter {
        Task Announce(IEnumerable<MetricConfig> configs, CancellationToken ct);

        Task Notify(SuccessfulMeasurement measurement, CancellationToken ct);

        Task Start(CancellationToken ct);

        Task Stop(CancellationToken ct);
    }
}