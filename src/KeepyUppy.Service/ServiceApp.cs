using System;

namespace KeepyUppy.Service
{
    public class ServiceApp : IServiceApp
    {
        public void StartService()
        {
            // connect to token available stream

            // if yes, grab token

            // if get token, activate

            // if not, do nothing

            Activate();
        }

        private static void Activate()
        {
            while (Console.ReadLine() != "die")
            {
                Console.WriteLine("Service still running happily =]");
            }

            throw new ApplicationException("oh noes service is dying");
        }
    }
}