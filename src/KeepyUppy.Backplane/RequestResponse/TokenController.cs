using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Web.Http;
using KeepyUppy.Backplane.Broadcast;
using KeepyUppy.Interop;
using log4net;

namespace KeepyUppy.Backplane.RequestResponse
{
    public class TokenController : ApiController
    {
        private readonly IBroadcaster _broadcaster;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object IdLock = new object();
        private static readonly object TokenLock = new object();
        private static bool _tokenAvailable = true;
        private static int _nextServiceId;
        private readonly Subject<Unit> _heartBeatStream = new Subject<Unit>();
        private readonly SerialDisposable _heartBeatSubscription = new SerialDisposable();

        public TokenController(IBroadcaster broadcaster)
        {
            _broadcaster = broadcaster;
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
                    _broadcaster.UpdateTokenAvailability(_tokenAvailable);
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
        public IHttpActionResult AcquireToken()
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
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private void StartHeartBeatMonitor()
        {
            _heartBeatSubscription.Disposable = _heartBeatStream.StartWith(Unit.Default).Buffer(TimeSpan.FromSeconds(5)).Subscribe(buffer =>
            {
                if (buffer.Any())
                {
                    Logger.Info("Heartbeat received");
                }
                else
                {
                    Logger.Warn("No Heartbeat - looking for a warm service...");
                    lock (TokenLock)
                    {
                        _heartBeatSubscription.Disposable.Dispose();
                        _tokenAvailable = true;
                        _broadcaster.UpdateTokenAvailability(_tokenAvailable);
                    }
                }
            });
        }

        [HttpPost]
        [Route(ApiRoutes.ReturnToken)]
        public IHttpActionResult ReturnToken()
        {
            try
            {
                lock (TokenLock)
                {
                    if (_tokenAvailable)
                    {
                        return Ok(false);
                    }

                    _tokenAvailable = true;
                    _broadcaster.UpdateTokenAvailability(_tokenAvailable);
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
                _heartBeatStream.OnNext(Unit.Default);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}