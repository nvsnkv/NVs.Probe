using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using NVs.Probe.Contract;

namespace NVs.Probe.Client
{
    internal sealed class PipeClient : IPipeClient
    {
        private readonly string name;
        private readonly IConsole console;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly NamedPipeClientStream pipe;

        public PipeClient(string name, IConsole console)
        {
            this.name = name;
            this.console = console ?? throw new ArgumentNullException(nameof(console));
            pipe = new NamedPipeClientStream(name);
        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel();
            await pipe.DisposeAsync();
        }

        public async Task<Response> Send(Request request)
        {
            console.WriteVerbose($"Pipe: sending {request} to instance {name}.");
            var buffer = new byte[1];
            buffer[0] = (byte)request;

            console.WriteVerbose("Pipe: Connecting...");
            await pipe.ConnectAsync(cts.Token);
            
            console.WriteVerbose("Pipe: Sending request...");
            await pipe.WriteAsync(buffer, cts.Token);
            
            console.WriteVerbose("Pipe: Reading response...");
            if (await pipe.ReadAsync(buffer, cts.Token) != 1)
            {
                console.WriteError("Pipe: incorrect response length received!");
                return Response.Unknown;
            }

            var response = (Response) buffer[0];
            console.WriteVerbose($"Pipe: {response} received.");
            return response;
        }
    }

    internal sealed class PipeClientBuilder
    {
        private readonly IConsole console;
        public PipeClientBuilder(IConsole console) => this.console = console;

        public IPipeClient Build(string name) => new PipeClient(name, console);
    }
}