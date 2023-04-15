using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public class Worker
    {
        private readonly System.Timers.Timer _timer;
        private Serilog.ILogger _logger;
        private ActivitySchedulerWorkerApp _app;
        public Worker(Serilog.ILogger logger, ActivitySchedulerWorkerApp app)
        {
            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += ExecuteEvent;
            _logger = logger;
            _app= app;
        }

        private void ExecuteEvent(object sender, ElapsedEventArgs e)
        {
            _logger.Information("This is topshelf worker logs");
        }

        public void Stop()
        {
            _timer.Stop();
        }
        public void Start()
        {
            _timer.Start();
        }

        public void RunService()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "net";
            startInfo.Arguments = $" start {_app.WinServiceName}";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
