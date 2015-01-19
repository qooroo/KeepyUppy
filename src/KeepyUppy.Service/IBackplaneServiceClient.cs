using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using System.Reactive;

namespace KeepyUppy.Service
{
    public interface IBackplaneServiceClient
    {
        IObservable<ConnectionState> ConnectionStateStream { get; }
        IObservable<string> ServerMessageStream { get; }
        IObservable<Unit> TokenAvailabilityStream { get; }

        Task Connect();
    }
}