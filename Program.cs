using System;
using System.Configuration;
using System.Threading;
using Microsoft.Owin.Hosting;

namespace abWebapi
{
    internal class Program
    {
        private static readonly ManualResetEvent _ExitEvent = new ManualResetEvent(false);

        private static void Main()
        {
            // Handle exit on CTRL+C
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                _ExitEvent.Set();
            };

            // Start OWIN host 
            using (WebApp.Start<Startup>(ConfigurationManager.AppSettings["ListenHost"]))
            {
                _ExitEvent.WaitOne();
            }
        }
    }
}