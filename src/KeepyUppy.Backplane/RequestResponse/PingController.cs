using System;
using System.Web.Http;

namespace KeepyUppy.Backplane.RequestResponse
{
    public class PingController : ApiController
    {
        [HttpPost]
        [Route("ping")]
        public IHttpActionResult Do(string request)
        {
            try
            {
                return Ok("Pong: " + request);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}