using System;
using Autofac;
using KeepyUppy.Interop;

namespace KeepyUppy.Service
{
    public class ServiceBootstrapper : IDisposable
    {
        public void Run()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TestUrlProvider>().As<IUrlProvider>().SingleInstance();
            builder.RegisterType<HttpServiceClient>().As<IHttpServiceClient>().SingleInstance();
            builder.RegisterType<ServiceApp>().As<IServiceApp>().SingleInstance();

            var container = builder.Build();

            container.Resolve<IServiceApp>().StartService();
        }

        public void Dispose()
        {
            
        }
    }
}