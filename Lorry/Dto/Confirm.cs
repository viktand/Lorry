using Newtonsoft.Json;
using System;

namespace Lorry.Dto
{
    public class Confirm
	{
		[JsonProperty("ready_to_trip")]
		public bool Ready { get; set; }

		[JsonProperty("version")]
		public int Version { get => 1; }

		[JsonProperty("cache")]
		public long Cache { get => DateTime.Now.Ticks; }
	}
}