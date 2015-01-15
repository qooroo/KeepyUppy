using System;
using Microsoft.Owin.Hosting;

namespace KeepyUppy.Backplane
{
    public class Bootstrapper : IDisposable
    {
        private IDisposable _webApp;

        public void Run()
        {
            _webApp = WebApp.Start<Startup>("http://localhost:8998");
        }

        public void Dispose()
        {
            _webApp.Dispose();
        }
    }
}