using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using System.Reactive;

namespace KeepyUppy.Service
{
    public interface IBackplaneServiceClient
    {
        IObservable<ConnectionState> ConnectionStateStream { get; }
        IObservable<bool> TokenAvailabilityStream { get; }

        Task Connect();
        Task<bool> RequestToken(int serviceId);
        Task<int> GetServiceAppId();
        void SendHeartBeat();
    }
}