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

        private readonly HubConnection _hubConnection;
        private readonly IHubProxy _hubProxy;
        private readonly IUrlProvider _urlProvider;
        private readonly IHttpServiceClient _httpServiceClient;

        public IObservable<Unit> TokenAvailabilityStream { get; }
        public IObservable<ConnectionState> ConnectionStateStream { get; }
        public IObservable<string> ServerMessageStream { get; }

        public BackplaneServiceClient(IUrlProvider urlProvider, IHttpServiceClient httpServiceClient)
        {
            _urlProvider = urlProvider;
            _httpServiceClient = httpServiceClient;

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

        public async Task<bool> RequestToken()
        {
            var hasToken = await _httpServiceClient.GetAsync<bool>(ApiRoutes.AcquireToken);

            Logger.InfoFormat("Get token result: {0}", hasToken ? "YUP, GOT IT" : "TOKEN NOT AVAILABLE");

            return hasToken;
        }

        public async Task<int> GetServiceAppId()
        {
            var id = await _httpServiceClient.GetAsync<int>(ApiRoutes.GetId);

            Logger.InfoFormat("Received Allocated ID: {0}", id);

            return id;
        }
    }
}
