using System.ServiceProcess;

namespace OffsiteBackupService
{
    public partial class ServiceClass : ServiceBase
    {
        public ServiceClass()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
