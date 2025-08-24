using Newtonsoft.Json;

namespace Ameria.Maverick
{
    public class Package
    {
        [JsonProperty("$schema", Order = int.MinValue)]
        public string Schema { get; set; }

        [JsonProperty("id", Order = int.MinValue + 1)]
        public string Id { get; set; }

        [JsonProperty("version", Order = int.MinValue + 2)]
        public string Version { get; set; }

        [JsonProperty("title", Order = int.MinValue + 3)]
        public string Title { get; set; }

        [JsonProperty("icon", Order = int.MinValue + 4)]
        public string Icon { get; set; }

        [JsonProperty("content", Order = int.MinValue + 5)]
        public string Content { get; set; }

        [JsonProperty("dependencies", NullValueHandling = NullValueHandling.Ignore, Order = int.MinValue + 6)]
        public string[] Dependencies { get; set; }

        [JsonProperty("exec", NullValueHandling = NullValueHandling.Ignore, Order = int.MinValue + 7)]
        public string[] Executable { get; set; }

        [JsonProperty("install", NullValueHandling = NullValueHandling.Ignore, Order = int.MinValue + 8)]
        public string[] Install { get; set; }

        [JsonProperty("StartType", Order = int.MinValue + 9)]
        public string StartType { get; set; }

        [JsonProperty("RestartBehaviour", Order = int.MinValue + 10)]
        public string RestartBehaviour { get; set; }

        public Package()
        {
        }
    }
}