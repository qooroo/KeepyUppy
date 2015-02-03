using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using KeepyUppy.Interop;
using Xunit;

namespace KeepyUppy.Test
{
    public class BackplaneIntegrationTests
    {
        private readonly HttpServiceClient _httpClient;
        private const string ServiceExe = @"..\..\..\KeepyUppy.Service\bin\Debug\KeepyUppy.Service.exe";
        private const string BackplaneExe = @"..\..\..\KeepyUppy.Backplane\bin\Debug\KeepyUppy.Backplane.exe";

        public BackplaneIntegrationTests()
        {
            _httpClient = new HttpServiceClient(new TestUrlProvider());
        }

        [Fact]
        public void WhenBackplaneStarts_ThenTokenIsAvailableAndNoHotServiceIsAttached()
        {
            Process backplane = null;
            try
            {
                backplane = Process.Start(BackplaneExe);
                Thread.Sleep(1000);
                var tokenAvailable = _httpClient.GetAsync<bool>(ApiRoutes.GetTokenAvailabilityStatus).Result;
                var hotServiceId = _httpClient.GetAsync<int>(ApiRoutes.GetHotServiceId).Result;
                
                tokenAvailable.Should().BeTrue();
                hotServiceId.Should().Be(0);
            }
            finally
            {
                backplane.Kill();
            }
        }

        [Fact]
        public void WhenServiceAndBackplaneAreRunning_ThenBackplaneHasHotServiceAttached()
        {
            Process backplane = null;
            Process service = null;
            try
            {
                backplane = Process.Start(BackplaneExe);
                Thread.Sleep(1000);
                service = Process.Start(ServiceExe);
                Thread.Sleep(3000);
                var tokenAvailability = _httpClient.GetAsync<bool>(ApiRoutes.GetTokenAvailabilityStatus).Result;
                var hotServiceId = _httpClient.GetAsync<int>(ApiRoutes.GetHotServiceId).Result;

                tokenAvailability.Should().BeFalse();
                hotServiceId.Should().NotBe(0);
            }
            finally
            {
                backplane.Kill();
                service.Kill();
            }
        }

        [Fact]
        public void GivenServicesAreRunningHotAndWarm_WhenHotServiceDies_ThenWarmActivates()
        {
            Process backplane = null;
            Process service1 = null;
            Process service2 = null;
            try
            {
                // start backplane and service, wait for connections
                backplane = Process.Start(BackplaneExe);
                Thread.Sleep(1000);
                service1 = Process.Start(ServiceExe);
                Thread.Sleep(3000);
                // get connected id
                var service1Id = _httpClient.GetAsync<int>(ApiRoutes.GetHotServiceId).Result;
                // start warm service
                service2 = Process.Start(ServiceExe);
                Thread.Sleep(3000);
                // hot id should still be the same
                var firstHotId = _httpClient.GetAsync<int>(ApiRoutes.GetHotServiceId).Result;
                firstHotId.Should().Be(service1Id);
                // kill hot service and wait for warm service to activate
                service1.Kill();
                Thread.Sleep(5000);

                var tokenAvailable = _httpClient.GetAsync<bool>(ApiRoutes.GetTokenAvailabilityStatus).Result;
                var latestHotId = _httpClient.GetAsync<int>(ApiRoutes.GetHotServiceId).Result;

                tokenAvailable.Should().BeFalse();
                latestHotId.Should().NotBe(firstHotId);
            }
            finally
            {
                backplane.Kill();
                service2.Kill();
            }
        }
    }
}
