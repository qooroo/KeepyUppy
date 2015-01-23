using System;
using log4net.Config;
using Topshelf;

namespace KeepyUppy.Service
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("\r\nSTARTING RESILIENT SERVICE\r\n");

            HostFactory.Run(x =>
            {
                x.Service<ServiceBootstrapper>(b =>
                {
                    b.ConstructUsing(_ => new ServiceBootstrapper());
                    b.WhenStarted(bs =>
                    {
                        XmlConfigurator.Configure();
                        bs.Run();
                    });
                    b.WhenStopped(bs => bs.Dispose());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("KeepyUppy.Service");
                x.SetDisplayName("KeepyUppy.Service");
            });

            Console.ReadLine();
        }
    }
}
