using System;
using System.Threading.Tasks;
using NVs.Probe.Contract;

namespace NVs.Probe.Client
{
    internal interface IPipeClient : IAsyncDisposable
    {
        Task<Response> Send(Request request);
    }
}