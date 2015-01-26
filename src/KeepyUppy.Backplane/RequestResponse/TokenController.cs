using System;
using System.Reactive.Linq;
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

        public TokenController(IBroadcaster broadcaster)
        {
            _broadcaster = broadcaster;
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(
                    _ => _broadcaster.BroadcastMessage(string.Format("clock: {0}", DateTime.Now.ToString("HH:mm:ss"))));
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

        [HttpPost]
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
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}