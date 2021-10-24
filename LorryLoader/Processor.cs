using Common;
using Common.Dto;
using System;

namespace LorryLoader
{
    internal class Processor : IProcessor
    {
        public string FarmIndex { get; set; }
        public string CurentField { get; set; }
        public string LastField { get; set; }

        public event EventHandler<LoadRequest> NewConnection;

        public LoadResponse TryConnect(LoadRequest r)
        {
            if(FarmIndex != r.FarmIndex)
            {
                return new LoadResponse
                {
                    Message = "",
                    Status = LoaderMessages.WrongPlaceOfLoading
                };
            }

            NewConnection?.Invoke(this, r);
            return new LoadResponse
            {
                Status = LoaderMessages.Ok
            };
        }
    }
}