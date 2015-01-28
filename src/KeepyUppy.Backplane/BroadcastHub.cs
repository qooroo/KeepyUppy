using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.SignalR;

namespace KeepyUppy.Backplane
{
    public class BroadcastHub : Hub
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public BroadcastHub()
        {
            
        }

        public override Task OnConnected()
        {
            Logger.InfoFormat("Connected: {0}", Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            Logger.InfoFormat("Reconnected: {0}", Context.ConnectionId);
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Logger.InfoFormat("Disconnected: {0}", Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }
}