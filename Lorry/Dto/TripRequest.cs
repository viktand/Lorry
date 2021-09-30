using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lorry.Dto
{
    public class TripRequest: CacheProperty
	{
		[JsonProperty("car_id")]
		public int Car { get; set; }

		[JsonProperty("isNeedDelete")]
		public bool IsNeedDelete { get; set; }

		[JsonProperty("ready_at")]
		public string ReadyAt { get => DateTime.Now.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT+0300'", CultureInfo.CreateSpecificCulture("en-US")); }  // "Thu Sep 30 2021 08:53:00 GMT+0300",
	}
}