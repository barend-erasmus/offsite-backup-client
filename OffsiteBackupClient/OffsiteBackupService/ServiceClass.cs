using log4net;
using OffsiteBackupClient;
using OffsiteBackupClient.Gateways;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;

namespace OffsiteBackupService
{
    public partial class ServiceClass : ServiceBase
    {
        private System.Timers.Timer _timer;
        private readonly ILog _log = LogManager.GetLogger(typeof(ServiceClass));

        public ServiceClass()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _timer = new System.Timers.Timer(new TimeSpan(0, Convert.ToInt32(ConfigurationManager.AppSettings["IntervalInMinutes"]), 0).TotalMilliseconds);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(Elapsed);

            _timer.Start();
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();

            _log.Info("Elapsed()");

            string[] gatewayTypes = ConfigurationManager.AppSettings["GatewayTypes"].Split(',');

            IList<IGateway> gateways = new List<IGateway>();

            if (gatewayTypes.Contains("Dropbox"))
            {
                gateways.Add(new DropboxGateway(ConfigurationManager.AppSettings["DropboxAccessToken"]));
            }

            if (gatewayTypes.Contains("SimpleCloudStorage"))
            {
                gateways.Add(new SimpleCloudStorageGateway(ConfigurationManager.AppSettings["SimpleCloudStorageProfileId"], ConfigurationManager.AppSettings["SimpleCloudStorageUri"]));
            }

            Client client = new Client(gateways.ToArray(), Convert.ToInt32(ConfigurationManager.AppSettings["BufferSize"]));

            client.UploadDirectory(ConfigurationManager.AppSettings["Path"], null);

            _timer.Start();

        }
    }
}
