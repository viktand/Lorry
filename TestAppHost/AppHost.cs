using System;
using ServiceStack;
using Funq;

namespace TestAppHost
{
    public class AppHost : AppSelfHostBase
    {
        private Processor _processor;

        public event EventHandler<LoadRequest> NewDriverConnection;
     
        public AppHost(): base("Loader Service", typeof(LoaderService).Assembly) { }

        public override void Configure(Container container)
        {
            _processor = new Processor
            {
                FarmIndex = "ТСТ"
            };
            container.AddSingleton<IProcessor>(_processor);

            _processor.NewConnection += NewConnection;
        }

        private void NewConnection(object sender, LoadRequest e)
        {
            NewDriverConnection?.Invoke(sender, e);
        }
    }
}