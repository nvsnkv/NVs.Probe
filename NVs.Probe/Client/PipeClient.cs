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
        private readonly TimeSpan timeout;
        private readonly IConsole console;
        private readonly CancellationTokenSource cts = new();
        private readonly NamedPipeClientStream pipe;

        public PipeClient(string name, TimeSpan timeout, IConsole console)
        {
            this.name = name;
            this.timeout = timeout;
            this.console = console ?? throw new ArgumentNullException(nameof(console));
            pipe = new NamedPipeClientStream(name);
        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel();
            await pipe.DisposeAsync();
        }

        public async Task<Response?> Send(Request request)
        {
            console.WriteVerbose($"Pipe: sending {request} to instance {name}.");
            var buffer = new byte[1];
            buffer[0] = (byte)request;

            console.WriteVerbose("Pipe: Connecting...");
            if (await pipe.ConnectAsync(cts.Token).TimeoutAfter(timeout, cts.Token))
            {
                console.WriteVerbose($"Pipe: connection request timed out !");
                return null;
            };
            
            console.WriteVerbose("Pipe: Sending request...");
            if (await pipe.WriteAsync(buffer, cts.Token).TimeoutAfter(timeout, cts.Token))
            {
                console.WriteVerbose($"Pipe: data transfer timed out after {timeout}!");
                return null;
            };
            
            console.WriteVerbose("Pipe: Reading response...");
            var bytesRed = await pipe.ReadAsync(buffer, cts.Token).TimeoutAfter(timeout, cts.Token);
            if (!bytesRed.HasValue)
            {
                console.WriteVerbose($"Pipe: data transfer timed out after {timeout}!");
                return null;
            }

            if (bytesRed.Value != 1)
            {
                console.WriteVerbose("Pipe: incorrect response length received!");
                return Response.Unknown;
            }

            var response = (Response) buffer[0];
            console.WriteVerbose($"Pipe: {response} received.");
            return response;
        }
    }

    /// <summary>
    /// A good hack from https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/. Please notice that it's not performance-efficient!
    /// </summary>
    internal static class TimeoutHelper
    {
        public static async Task<T?> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout, CancellationToken token) where T : struct
        {
            return task == await Task.WhenAny(task, Task.Delay(timeout, token)) 
                ? (T?) await task 
                : null;
        }

        public static async ValueTask<T?> TimeoutAfter<T>(this ValueTask<T> valueTask, TimeSpan timeout, CancellationToken token) where T : struct
        {
            var task = valueTask.AsTask();
            return task == await Task.WhenAny(task, Task.Delay(timeout, token))
                ? (T?)await valueTask
                : null;
        }

        public static async Task<bool> TimeoutAfter(this Task task, TimeSpan timeout, CancellationToken token)
        {
            return task != await Task.WhenAny(task, Task.Delay(timeout, token));
        }
        
        public static async ValueTask<bool> TimeoutAfter(this ValueTask valueTask, TimeSpan timeout, CancellationToken token)
        {
            var task = valueTask.AsTask();
            return task != await Task.WhenAny(task, Task.Delay(timeout, token));
        }
    }

    internal sealed class PipeClientBuilder
    {
        private readonly IConsole console;
        private readonly TimeSpan timeout;

        public PipeClientBuilder(IConsole console, TimeSpan timeout)
        {
            this.console = console ?? throw new ArgumentNullException(nameof(console));
            this.timeout = timeout;
        }

        public IPipeClient Build(string name) => new PipeClient(name, timeout, console);
    }
}