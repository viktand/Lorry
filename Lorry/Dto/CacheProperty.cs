using Newtonsoft.Json;
using System;

namespace Lorry.Dto
{
    public class CacheProperty
    {
        [JsonProperty("version")]
        public int Version { get => 1; }

        [JsonProperty("cache")]
        public long Cache { get => DateTime.Now.Ticks; }
    }
}