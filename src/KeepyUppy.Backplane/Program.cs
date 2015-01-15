using System;
using log4net.Config;
using Topshelf;

namespace KeepyUppy.Backplane
{
    class Program
    {
        static void Main()
        {
            HostFactory.Run(x =>
            {
                x.Service<Bootstrapper>(b =>
                {
                    b.ConstructUsing(_ => new Bootstrapper());
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
