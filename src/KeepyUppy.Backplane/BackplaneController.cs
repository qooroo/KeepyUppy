using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Web.Http;
using KeepyUppy.Interop;
using log4net;

namespace KeepyUppy.Backplane
{
    public class BackplaneController : ApiController
    {
        private readonly IBroadcaster _broadcaster;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object IdLock = new object();
        private static readonly object TokenLock = new object();
        private static bool _tokenAvailable = true;
        private static int _nextServiceId;
        private static readonly Subject<Unit> HeartBeatStream = new Subject<Unit>();
        private static readonly SerialDisposable HeartBeatSubscription = new SerialDisposable();
        private static int _hotServiceId;

        public BackplaneController(IBroadcaster broadcaster)
        {
            _broadcaster = broadcaster;
        }

        public void BroadcastTokenAvailabilty()
        {
            _broadcaster.BroadcastTokenAvailability(_tokenAvailable);
        }

        [HttpGet]
        [Route(ApiRoutes.GetTokenAvailabilityStatus)]
        public IHttpActionResult GetTokenAvailabilityStatus()
        {
            return Ok(_tokenAvailable);
        }

        [HttpGet]
        [Route(ApiRoutes.GetHotServiceId)]
        public IHttpActionResult GetHotServiceId()
        {
            return Ok(_hotServiceId);
        }

        [HttpGet]
        [Route(ApiRoutes.GetId)]
        public IHttpActionResult GetId()
        {
            Logger.Info("Received GetId request");
            try
            {
                lock (IdLock)
                {
                    return Ok(++_nextServiceId);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route(ApiRoutes.AcquireToken)]
        public IHttpActionResult AcquireToken(int serviceId)
        {
            Logger.Info("Received AcquireToken request");
            try
            {
                lock (TokenLock)
                {
                    if (!_tokenAvailable)
                    {
                        return Ok(false);
                    }

                    _tokenAvailable = false;
                    StartHeartBeatMonitor();
                    _hotServiceId = serviceId;
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route(ApiRoutes.HeartBeat)]
        public IHttpActionResult HeartBeat()
        {
            try
            {
                Logger.Info("Received heartbeat");
                HeartBeatStream.OnNext(Unit.Default);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private void StartHeartBeatMonitor()
        {
            HeartBeatSubscription.Disposable = HeartBeatStream.StartWith(Unit.Default).Buffer(TimeSpan.FromSeconds(2)).Subscribe(buffer =>
            {
                Logger.WarnFormat("{0} beats in buffer", buffer.Count);
                if (buffer.Any())
                {
                    // all good, nothing to do, as you were.
                }
                else
                {
                    Logger.Warn("No Heartbeat - looking for a warm service...");
                    lock (TokenLock)
                    {
                        HeartBeatSubscription.Disposable.Dispose();
                        _tokenAvailable = true;
                        _hotServiceId = 0;
                        _broadcaster.BroadcastTokenAvailability(_tokenAvailable);
                    }
                }
            });
        }
    }
}