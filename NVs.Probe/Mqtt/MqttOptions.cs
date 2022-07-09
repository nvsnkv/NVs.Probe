using MQTTnet.Client;

namespace NVs.Probe.Mqtt
{
    internal class MqttOptions 
    {
        public MqttOptions(MqttClientOptions clientOptions, RetryOptions retryOptions)
        {
            ClientOptions = clientOptions;
            RetryOptions = retryOptions;
        }

        public MqttClientOptions ClientOptions { get; }

        public RetryOptions RetryOptions { get; }
    }
}