using System;
using System.Reactive.Linq;
using System.Reflection;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace KeepyUppy.Backplane.Broadcast
{
    public class Broadcaster : IBroadcaster
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IHubContext _context;

        public Broadcaster(IConnectionManager connectionManager)
        {
            _context = connectionManager.GetHubContext<BroadcastHub>();

            //Observable.Interval(TimeSpan.FromSeconds(5))
            //    .Subscribe(_ => BroadcastMessage(DateTime.Now.ToString("HH:mm:ss")));
        }

        public void BroadcastMessage(string message)
        {
            Logger.InfoFormat("Broadcasting message: {0}", message);
            _context.Clients.All.OnMessage(message);
        }

        public void UpdateTokenAvailability(bool tokenAvailable)
        {
            Logger.InfoFormat("Broadcasting token availability: {0}", tokenAvailable);
            _context.Clients.All.OnTokenAvailability(tokenAvailable);
        }
    }
}
