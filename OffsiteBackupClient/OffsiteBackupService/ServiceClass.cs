using OffsiteBackupClient;
using OffsiteBackupClient.Gateways;
using System;
using System.Configuration;
using System.ServiceProcess;

namespace OffsiteBackupService
{
    public partial class ServiceClass : ServiceBase
    {
        private System.Timers.Timer _timer;

        public ServiceClass()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _timer = new System.Timers.Timer(new TimeSpan(12, 0, 0).TotalMilliseconds);
            _timer.AutoReset = true;
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
            IGateway gateway = new DropboxGateway(ConfigurationManager.AppSettings["DropboxAccessToken"]);
            Client client = new Client(gateway, Convert.ToInt32(ConfigurationManager.AppSettings["BufferSize"]));

            client.UploadDirectory(ConfigurationManager.AppSettings["Path"], null);
        }
    }
}
