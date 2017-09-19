using log4net.Config;
using System.ServiceProcess;

namespace OffsiteBackupService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            XmlConfigurator.Configure();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServiceClass()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
