using CommandLine;

namespace NVs.Probe.Setup 
{
    internal sealed class Options 
    {
        [Option('c', Required = true, HelpText = "Client identified for MQTT broker.")]
        public string ClientId { get; }

        //[Option('s', Required = false, HelpText = "Address of MQTT broker")]
        public string Server { get; }
 
        //[Option(Required = false, HelpText = "Port number of MQTT broker", Default = 1883)]    
        public int Port { get; }

        //[Option('u', Required = true, HelpText = "Username used for authentication on MQTT broker")]
        public string User { get; }

        //[Option('p', Required = true, HelpText = "Password user for authentication on MQTT broker")]
        public string Password { get; }

        //[Option('t', Required = false, HelpText = "Timeout in milliseconds for a single measurement", Default = 1000)]
        public long MeasurementTimeout { get; }

        //[Option('i', Required = false, HelpText = "Interval in milliseconds between the series of measurement", Default = 120000)]
        public long SeriesInterval { get; }
        
        public Options(string clientId)//, string server, int port, string user, string password, long measurementTimeout, long seriesInterval)
        {
            ClientId = clientId;
            Server = server;
            Port = port;
            User = user;
            Password = password;
            MeasurementTimeout = measurementTimeout;
            SeriesInterval = seriesInterval;
        }
    }
}