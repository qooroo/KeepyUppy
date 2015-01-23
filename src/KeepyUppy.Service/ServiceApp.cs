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
            _serviceAppId = await _backplaneServiceClient.GetServiceAppId();
            _serviceAppSubscriptions.Add(_backplaneServiceClient.TokenAvailabilityStream.Subscribe(_ => OnTokenAvailable()));
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