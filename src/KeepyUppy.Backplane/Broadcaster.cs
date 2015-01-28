using System.Reflection;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace KeepyUppy.Backplane
{
    public class Broadcaster : IBroadcaster
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IHubContext _context;

        public Broadcaster(IConnectionManager connectionManager)
        {
            _context = connectionManager.GetHubContext<BroadcastHub>();
        }

        public void BroadcastTokenAvailability(bool tokenAvailable)
        {
            Logger.InfoFormat("Broadcasting token availability: {0}", tokenAvailable);
            _context.Clients.All.OnTokenAvailability(tokenAvailable);
        }
    }
}
