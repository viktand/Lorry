using Newtonsoft.Json;
using System;

namespace Lorry.Dto
{
    public class Confirm: CacheProperty
	{
		[JsonProperty("ready_to_trip")]
		public bool Ready { get; set; }		
	}
}