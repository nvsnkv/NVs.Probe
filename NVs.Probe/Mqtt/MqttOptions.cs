using MQTTnet.Client.Options;

namespace NVs.Probe.Mqtt
{
    internal class MqttOptions 
    {
        public MqttOptions(IMqttClientOptions clientOptions, RetryOptions retryOptions)
        {
            ClientOptions = clientOptions;
            RetryOptions = retryOptions;
        }

        public IMqttClientOptions ClientOptions { get; }

        public RetryOptions RetryOptions { get; }
    }
}