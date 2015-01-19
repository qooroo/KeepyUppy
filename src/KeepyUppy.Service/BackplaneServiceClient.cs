using KeepyUppy.Interop;
using log4net;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KeepyUppy.Service
{
    public class BackplaneServiceClient : IBackplaneServiceClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private readonly IUrlProvider _urlProvider;

        public IObservable<Unit> TokenAvailabilityStream { get; private set; }
        public IObservable<ConnectionState> ConnectionStateStream { get; private set; }
        public IObservable<string> ServerMessageStream { get; private set; }

        public BackplaneServiceClient(IUrlProvider urlProvider)
        {
            _urlProvider = urlProvider;

            _hubConnection = new HubConnection(_urlProvider.BackplaneUrl, true);

            _hubProxy = _hubConnection.CreateHubProxy("BroadcastHub");

            ConnectionStateStream = Observable.Create<ConnectionState>(observer =>
            {
                observer.OnNext(ConnectionState.Disconnected);

                return Observable.FromEvent<StateChange>(
                    h => _hubConnection.StateChanged += h,
                    h => _hubConnection.StateChanged -= h)
                    .Subscribe(s => observer.OnNext(s.NewState));
            });

            ServerMessageStream = Observable.Create<string>(observer => _hubProxy.On<string>("OnMessage", observer.OnNext));
            TokenAvailabilityStream = Observable.Create<bool>(observer => _hubProxy.On<bool>("OnTokenAvailability", observer.OnNext)).Where(b => b).Select(_ => Unit.Default);
        }
        public Task Connect()
        {
            Logger.InfoFormat("Connecting to backplane service at {0}", _urlProvider.BackplaneUrl);
            return _hubConnection.Start();
        }
    }
}
