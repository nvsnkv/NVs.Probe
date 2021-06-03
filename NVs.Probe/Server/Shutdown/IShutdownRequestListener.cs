using System;

namespace NVs.Probe.Server.Shutdown
{
    internal interface IShutdownRequestListener : IAsyncDisposable
    {
        event EventHandler ShutdownRequested;
    }
}