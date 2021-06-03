namespace NVs.Probe.Contract
{
    internal enum Request : byte
    {
        Unknown = 0,
        Ping = 1,
        Shutdown = 2
    };

    internal enum Response : byte
    {
        Unknown = 0,
        Pong = 3,
        Bye = 4
    }
}