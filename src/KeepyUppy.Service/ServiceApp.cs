using log4net;
using System;
using System.Reactive.Disposables;
using System.Reflection;

namespace KeepyUppy.Service
{
    public class ServiceApp : IServiceApp
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IBackplaneServiceClient _backplaneServiceClient;
        private readonly CompositeDisposable _serviceAppSubscriptions = new CompositeDisposable();
        private int _serviceAppId;

        public ServiceApp(IBackplaneServiceClient backplaneServiceClient)
        {
            _backplaneServiceClient = backplaneServiceClient;
        }

        public async void StartService()
        {
            Logger.Info("Starting service - connecting to backplane streams");

            _serviceAppSubscriptions.Add(_backplaneServiceClient.TokenAvailabilityStream.Subscribe(_ => OnTokenAvailable()));
            _serviceAppSubscriptions.Add(_backplaneServiceClient.ServerMessageStream.Subscribe(msg => Logger.InfoFormat("Server Message: {0}", msg)));
            await _backplaneServiceClient.Connect();

            Logger.Info("Connected to backplane streams, requesting Id");

            _serviceAppId = await _backplaneServiceClient.GetServiceAppId();

            Logger.InfoFormat("Assigned Id: {0}", _serviceAppId);
        }

        private async void OnTokenAvailable()
        {
            if (await _backplaneServiceClient.RequestToken())
            {
                Logger.Info("Got token! Activating...");
                Activate();
            }
        }

        private static void Activate()
        {
            while (Console.ReadLine() != "die")
            {
                Console.WriteLine("Service still running happily =]");
            }

            throw new ApplicationException("oh noes service is dying");
        }

        public void StopService()
        {
            _serviceAppSubscriptions.Dispose();
        }
    }
}