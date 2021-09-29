using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lorry.Dto
{
    public class Log
    {
        public string arrivalTime { get; set; }
        public string responseTime { get; set; }
        public string loggingTime { get; set; }
        public string loadingTime { get; set; }
        public string status { get; set; }
        public int id { get; set; }
        public int isLoadingAccepted { get; set; }
        public int isLoadingSuccess { get; set; }
        public bool isReload { get; set; }
        public int isStatusReversed { get; set; }
        public int isWrongField { get; set; }
    }

    public class LogTrip
    {
        public List<Log> logs { get; set; }
        public List<object> loadedTrips { get; set; }
        public int driverId { get; set; }
        public string time { get; set; }
        public int version { get; set; }
        public long cache { get; set; }
    }
}