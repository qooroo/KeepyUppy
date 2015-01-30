using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KeepyUppy.Interop;
using Xunit;

namespace KeepyUppy.Test
{

    public class BackplaneTests
    {
        private readonly HttpServiceClient _httpClient;

        public BackplaneTests()
        {
            _httpClient = new HttpServiceClient(new TestUrlProvider());
        }

        [Fact]
        public void WhenBackplaneStartsThenTokenIsAvailable()
        {
            Process p1 = null;
            try
            {
                p1 = Process.Start(@"..\..\..\KeepyUppy.Backplane\bin\Debug\KeepyUppy.Backplane.exe");
                Thread.Sleep(1000);
                Assert.True(_httpClient.GetAsync<bool>(ApiRoutes.GetTokenAvailabilityStatus).Result);
            }
            finally
            {
                p1.Kill();
            }
        }

        [Fact]
        public void WhenServiceAndBackplaneAreRunningThenBackplaneActivatesHeartbeatMonitor()
        {
            Process p1 = null;
            Process p2 = null;
            try
            {
                p1 = Process.Start(@"..\..\..\KeepyUppy.Backplane\bin\Debug\KeepyUppy.Backplane.exe");
                p2 = Process.Start(@"..\..\..\KeepyUppy.Service\bin\Debug\KeepyUppy.Service.exe");
                Thread.Sleep(3000);
                Assert.True(_httpClient.GetAsync<bool>(ApiRoutes.GetHeartBeatMonitorStatus).Result);
            }
            finally
            {
                p1.Kill();
                p2.Kill();
            }
        }
    }
}
