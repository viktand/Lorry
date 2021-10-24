using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Lorry.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lorry
{
    public static class Extentions
    {
        public static Statuses ToStatusName(this string slug)
        {
            switch (slug)
            {
                case "new": return Statuses.Appointed;
                case "loaded": return Statuses.Loaded;
                case "finished": return Statuses.Ended;
                case "arrived": return Statuses.Arrived;
                case "on_factory": return Statuses.OnPlant;
                case "paused": return Statuses.Paused;
                case "canceled": return Statuses.Canseled;
                default: return Statuses.None;
            }             
        }

        public static bool NotEquals(this Dictionary<int, Statuses> x, Dictionary<int, Statuses> y)
        {
            if (x.Count != y.Count) return true;
            foreach(var t in x.Keys)
            {
                if(! y.ContainsKey(t) || x[t] != y[t]) return true;
            }
            return false;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            if (s == null) return true;
            return s.Trim().Equals("");
        }

        public static string ToClearPhone(this string phone)
        {
            phone = phone.Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "");
            return double.TryParse(phone, out var i) ? (i > 79000000000.0 ? phone : "") : "";
        }
    }
}