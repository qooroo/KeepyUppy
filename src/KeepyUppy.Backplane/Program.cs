using System;
using log4net.Config;
using Topshelf;

namespace KeepyUppy.Backplane
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("\r\nSTARTING BACKPLANE SERVICE\r\n");

            HostFactory.Run(x =>
            {
                x.Service<BackplaneBootstrapper>(b =>
                {
                    b.ConstructUsing(_ => new BackplaneBootstrapper());
                    b.WhenStarted(bs =>
                    {
                        XmlConfigurator.Configure();
                        bs.Run();
                    });
                    b.WhenStopped(bs => bs.Dispose());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("KeepyUppy.Backplane");
                x.SetDisplayName("KeepyUppy.Backplane");
            });

            Console.ReadLine();
        }
    }
}
