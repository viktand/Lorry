using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lorry.Dto
{
    public class LoginRequest: CacheProperty
	{
		[JsonProperty("phone")]
		public string Phone { get; set; }

		[JsonProperty("password")]
		public string Password { get; set; }

		[JsonProperty("token")]
		public string Token { get; set; }

		[JsonProperty("notification_token")]
		public string NotificationToken { get; set; }
	}
}