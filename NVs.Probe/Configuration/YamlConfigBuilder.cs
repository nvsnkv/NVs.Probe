using System;
using System.IO;
using YamlDotNet.Serialization;

namespace NVs.Probe.Configuration
{
    internal class YamlConfigBuilder<T>
    {
        private readonly IDeserializer deserializer;

        public YamlConfigBuilder(IYamlTypeConverter converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            deserializer = new DeserializerBuilder().WithTypeConverter(converter).Build();
        }

        public T Build(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file does not exists!", filePath);
            }

            using var rdr = new StreamReader(File.OpenRead(filePath));
            return Build(rdr);
        }

        public T Build(TextReader reader)
        {
            return deserializer.Deserialize<T>(reader);
        }
    }
}