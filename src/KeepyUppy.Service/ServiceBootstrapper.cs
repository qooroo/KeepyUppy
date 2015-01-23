using System;
using Autofac;
using KeepyUppy.Interop;

namespace KeepyUppy.Service
{
    public class ServiceBootstrapper : IDisposable
    {
        private IServiceApp _app;

        public void Run()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TestUrlProvider>().As<IUrlProvider>().SingleInstance();
            builder.RegisterType<HttpServiceClient>().As<IHttpServiceClient>().SingleInstance();
            builder.RegisterType<ServiceApp>().As<IServiceApp>().SingleInstance();
            builder.RegisterType<BackplaneServiceClient>().As<IBackplaneServiceClient>().SingleInstance();

            var container = builder.Build();

            _app = container.Resolve<IServiceApp>();
            _app.StartService();
        }

        public void Dispose()
        {
            _app.StopService();
        }
    }
}