using System;

namespace NVs.Probe.Mqtt
{
    internal sealed class RetryOptions
    {
        private readonly TimeSpan? interval;
        private readonly uint retriesCount;

        public RetryOptions(TimeSpan? interval, uint retriesCount = 0)
        {
            this.interval = interval;
            this.retriesCount = retriesCount;
        }

        public bool ShouldRetry => interval.HasValue;

        public TimeSpan Interval => interval ?? throw new InvalidOperationException("Retry is not configured!");

        public uint RetriesCount => interval.HasValue ? retriesCount : throw new InvalidOperationException("Retry is not configured!");
    }
}