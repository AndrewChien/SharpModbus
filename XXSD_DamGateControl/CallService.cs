using System.ServiceProcess;

namespace XXSD_DamGateControl
{
    public partial class CallService : ServiceBase
    {
        private RunningClass _running = null;
        public CallService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _running?.Stop();
            _running = new RunningClass();
            var isStart = _running.Start();
        }

        protected override void OnStop()
        {
            _running?.Stop();
            _running = null;
        }
    }
}
