using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public class WorkerContainer : ServiceControl
    {
        Worker _worker;
        private Serilog.ILogger _logger;
        private ActivitySchedulerWorkerApp _app;
        public WorkerContainer(Serilog.ILogger logger, ActivitySchedulerWorkerApp app)
        {
            _logger = logger;
            _logger.Information("Workes service business logic class constructor");
            _app= app;
            _logger.Information("Workes service business logic class constructor--passed ok");
            _worker = new Worker(_logger, _app);
        }

        public bool Start(HostControl hostControl)
        {
            Task task = Task.Run(()=>_worker.Start());
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Task task = Task.Run(() => _worker.Stop());
            return true;
        }
    }
}
