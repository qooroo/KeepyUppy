using log4net;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;

namespace KeepyUppy.Service
{
    public class ServiceApp : IServiceApp
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IBackplaneServiceClient _backplaneServiceClient;
        private readonly CompositeDisposable _serviceAppSubscriptions = new CompositeDisposable();
        private int _serviceAppId;
        private readonly SerialDisposable _heartBeatSubscription = new SerialDisposable();
        private bool _activated;

        public ServiceApp(IBackplaneServiceClient backplaneServiceClient)
        {
            _backplaneServiceClient = backplaneServiceClient;
        }

        public async void StartService()
        {
            Logger.Info("Connecting to backplane streams");

            _serviceAppSubscriptions.Add(_backplaneServiceClient.TokenAvailabilityStream.Subscribe(TryRequestToken));
            _serviceAppSubscriptions.Add(_backplaneServiceClient.ServerMessageStream.Subscribe(msg => Logger.InfoFormat("Server Message: {0}", msg)));
            await _backplaneServiceClient.Connect();

            Logger.Info("Requesting Id");
            _serviceAppId = await _backplaneServiceClient.GetServiceAppId();
            Logger.InfoFormat("Assigned Id: {0}, listening for token availability...", _serviceAppId);
        }

        private async void TryRequestToken(bool isTokenAvailable)
        {
            if (isTokenAvailable && !_activated && await _backplaneServiceClient.RequestToken())
            {
                Logger.Info("Got token! Activating...");
                Activate();
            }
        }

        private void Activate()
        {
            _activated = true;
            _heartBeatSubscription.Disposable = Observable.Interval(TimeSpan.FromSeconds(1))
                .ObserveOn(new NewThreadScheduler())
                .Subscribe(_ => _backplaneServiceClient.SendHeartBeat());

            // start real service work here

            Logger.Info("Service activated!");
        }

        public void StopService()
        {
            _serviceAppSubscriptions.Dispose();
            _heartBeatSubscription.Dispose();
        }
    }
}