using System.Threading;
using System.Threading.Tasks;

namespace NVs.Probe.Execution
{
    internal interface ICommandRunner
    {
        public Task<string> Execute(string command, CancellationToken ct);
    }
}