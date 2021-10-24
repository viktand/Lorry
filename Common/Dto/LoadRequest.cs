using System;

namespace Common.Dto
{
    public class LoadRequest
    {
        public string Plate { get; set; }
        public string Driver { get; set; }
        public string FarmIndex { get; set; }
        public string Field { get; set; }
        public DateTime LoadTime { get; set; }
    }

    public class LoadResponse
    {
        public string Message { get; set; }
        public LoaderMessages Status {get; set;}
    }
}