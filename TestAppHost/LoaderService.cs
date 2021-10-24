using ServiceStack;

namespace TestAppHost
{
    internal class LoaderService
    {
        public LoadResponse Any(LoadRequest r)
        {
            var proc = HostContext.Resolve<IProcessor>();
            return proc.TryConnect(r);
        }
    }
}