using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.WorkerService
{
    public class Worker : BackgroundService
    {
        Serilog.ILogger _logger;

        public Worker(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Information("Hello, World!");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
