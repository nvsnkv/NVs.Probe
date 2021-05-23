using System.IO;
using YamlDotNet.Serialization;

namespace NVs.Probe.Config
{
    internal sealed class YamlConfigBuilder<TConfig, TConverter> where TConverter : class, IYamlTypeConverter, new()
    {
        private readonly IDeserializer deserializer;

        public YamlConfigBuilder()
        {
            deserializer = new DeserializerBuilder().WithTypeConverter(new TConverter()).Build();
        }

        public TConfig Build(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file does not exists!", filePath);
            }

            using var rdr = new StreamReader(File.OpenRead(filePath));
            return Build(rdr);
        }

        public TConfig Build(TextReader reader)
        {
            return deserializer.Deserialize<TConfig>(reader);
        }
    }
}