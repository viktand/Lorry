using EmbedIO;
using EmbedIO.Security;
using EmbedIO.WebApi;
using LorryLoader.Controllers;
using System.Threading;

namespace LorryLoader
{
    /// <summary>
    /// Класс для подключения водителей
    /// </summary>
    public class Server
    {
        private string _url;
        private CancellationToken _cancellationToken;

        public  Server(string url)
        {
            _url = url;           
        }

        public async void Start()
        {
            using var server = CreateWebServer(_url);
            _cancellationToken = new CancellationToken();
            await server.RunAsync(_cancellationToken).ConfigureAwait(false);
        }

        private static WebServer CreateWebServer(string url)
        {
#pragma warning disable CA2000 // Call Dispose on object - this is a factory method.
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithIPBanning(o => o
                    .WithMaxRequestsPerSecond()
                    .WithRegexRules("HTTP exception 404"))
                .WithLocalSessionManager()
                .WithCors(
                    "http://unosquare.github.io,http://run.plnkr.co", // Origins, separated by comma without last slash
                    "content-type, accept", // Allowed headers
                    "post") // Allowed methods
                .WithWebApi("/api", m => m
                    .WithController<DriverAppController>());

            // Listen for state changes.
            server.StateChanged += Server_StateChanged;

            return server;
#pragma warning restore CA2000
        }

        private static void Server_StateChanged(object sender, WebServerStateChangedEventArgs e)
        {
            return;
        }
    }
}