using System;

namespace KeepyUppy.Service
{
    public class ServiceApp : IServiceApp
    {
        public void StartService()
        {
            while (Console.ReadLine() != "die")
            {
                Console.WriteLine("Service still running happily =]");
            }

            throw new ApplicationException("oh noes service is dying");
        }
    }
}