using System;

namespace Lorry.Dto
{
    public class ToLoaderMessage
    {
        public int id { get; set; }
        public string dqId { get; set; }
        public string status { get; set; }
        public string loadingDate { get; set; }
        public string loadingPeriod { get; set; }
        public string loadingPlace { get; set; }
        public string unloadingDate { get; set; }
        public string unloadingPeriod { get; set; }
        public string unloadingPlace { get; set; }
        public string selectedCar { get; set; }
        public string alternativeName { get; set; }
        public string farmName { get; set; }
        public string driver { get; set; }
        public string phone { get; set; }
        public string trinketId { get; set; }
        public LoadingStation loadingStation { get; set; }
        public bool isLoaderInUse { get; set; }
        public string alternative_name { get; set; }
        public string loadingStationName { get; set; }
        public string loadingStationId { get; set; }
        public DateTime time { get; set; }
    }

    public class LoadingStation
    {
        public int id { get; set; }
        public int farm_id { get; set; }
        public string name { get; set; }
        public object short_name { get; set; }
        public Farm farm { get; set; }
    }
}