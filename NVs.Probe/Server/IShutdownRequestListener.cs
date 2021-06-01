using System;
using System.Threading.Tasks;

namespace NVs.Probe.Server
{
    internal interface IShutdownRequestListener : IAsyncDisposable
    {
        event EventHandler ShutdownRequested;
    }

    internal sealed class ShutdownRequestListener : IShutdownRequestListener
    {
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public event EventHandler ShutdownRequested;
    }
}