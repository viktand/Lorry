using System;

namespace TestAppHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var appHost = new AppHost();
            appHost.Init();
            appHost.Start("http://*:500/");
            appHost.NewDriverConnection += AppHost_NewDriverConnection;
            while (true) ;
        }

        private static void AppHost_NewDriverConnection(object sender, LoadRequest e)
        {
            throw new NotImplementedException();
        }
    }
}
