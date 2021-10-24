using System;

namespace TestAppHost
{
    internal interface IProcessor
    {
        public string FarmIndex { get; set; }
        public string CurentField { get; set; }
        public string LastField { get; set; }

        public event EventHandler<LoadRequest> NewConnection;

        /// <summary>
        /// Попытка подключения нового водителя к погрузчику
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        LoadResponse TryConnect(LoadRequest r);
    }
}