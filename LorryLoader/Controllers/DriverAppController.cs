using Common.Dto;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System.Threading.Tasks;

namespace LorryLoader.Controllers
{
    public class DriverAppController : WebApiController
    {        
        // POST http://localhost:8877/api/connect
        [Route(HttpVerbs.Post, "/connect")]
        public Task<LoadResponse> GetAllPeople()
        {
            return Task.Run(() =>
            {
                return new LoadResponse
                {
                    Status = Common.LoaderMessages.Ok,
                    Message = "Вы добавлены в очередь"
                };
            });
        }
    }
}