using System;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using KeepyUppy.Backplane.Broadcast;
using KeepyUppy.Interop;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using Owin;

namespace KeepyUppy.Backplane
{
    public class BackplaneBootstrapper : IDisposable
    {
        private IDisposable _webApp;

        public void Run()
        {
            var builder = new ContainerBuilder();

            ExecuteRegistrations(builder);

            var container = builder.Build();

            _webApp = WebApp.Start(container.Resolve<IUrlProvider>().BackplaneUrl, app =>
            {
                app.UseWelcomePage("/");

                var config = new HttpConfiguration
                {
                    DependencyResolver = new AutofacWebApiDependencyResolver(container),
                    IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
                };
                config.MapHttpAttributeRoutes();
                app.UseWebApi(config);

                var resolver = new AutofacDependencyResolver(container);
                #region signalr dance to enable hub constructor DI

                var connectionManager = resolver.Resolve<IConnectionManager>();
                var updateBuilder = new ContainerBuilder();
                updateBuilder.RegisterInstance(connectionManager).As<IConnectionManager>();
                updateBuilder.Update(container.ComponentRegistry);

                #endregion
                app.MapSignalR(new HubConfiguration { Resolver = resolver, EnableDetailedErrors = true });
            });
        }

        private static void ExecuteRegistrations(ContainerBuilder builder)
        {
            builder.RegisterType<TestUrlProvider>().As<IUrlProvider>().SingleInstance();
            builder.RegisterType<Broadcaster>().As<IBroadcaster>().SingleInstance();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
        }

        public void Dispose()
        {
            _webApp.Dispose();
        }
    }
}