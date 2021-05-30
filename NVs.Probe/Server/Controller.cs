using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NVs.Probe.Server
{
    internal sealed class PipeController : IAsyncDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly IHostedService service;
        private readonly ILogger<PipeController> logger;
        private readonly NamedPipeServerStream stream;
        private readonly Task listener;

        public PipeController(string name, IHostedService service, ILogger<PipeController> logger)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.stream = new NamedPipeServerStream(name, PipeDirection.In);
            this.listener = Task.Factory.StartNew(Listen, cts.Token);
        }

        private async Task Listen()
        {
            using (logger.BeginScope("Listener task"))
            {
                logger.LogDebug("Controller started. Awaiting connections... ");
                while (!cts.IsCancellationRequested)
                {
                    await stream.WaitForConnectionAsync(cts.Token);
                    logger.LogDebug("New connection received!");
                    try
                    {
                        var ss = new StreamString(stream);
                        var command = await ss.ReadString();
                        switch (command)
                        {
                            case "stop":
                                logger.LogInformation("Request to stop server received, terminating application...");
                                stream.Close();
                                await service.StopAsync(cts.Token);
                                logger.LogInformation("Service stopped, terminating the host...");
                                Environment.Exit(0);
                                return;

                            default:
                            logger.LogWarning($"Unknown command received: {command}");
                            break;

                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to parse the command!");
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel(true);
            await listener;
            await stream.DisposeAsync();
            listener?.Dispose();
            cts.Dispose();
        }
    }

    /// <summary>
    /// Date protocol definition stolen from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication
    /// </summary>
    internal sealed class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public async Task<string> ReadString()
        {
            var len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            await ioStream.ReadAsync(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}