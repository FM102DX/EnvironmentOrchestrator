using ActivityScheduler.Shared;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Drawing;
using ActivityScheduler.WorkerService;
using Microsoft.Extensions.Hosting;

namespace ActivityScheduler.WindowsService
{
    public class Program
    {
        static void Main(string[] args)
        {

            ActivitySchedulerWorkerApp app = new ActivitySchedulerWorkerApp();

            string logFilePath = System.IO.Path.Combine(app.LogsDirectory, Functions.GetNextFreeFileName(app.LogsDirectory, "ActivitySchedulerLogs", "txt"));

            Serilog.ILogger _logger = new LoggerConfiguration()
                  .WriteTo.File(logFilePath)
                  .CreateLogger();

            IHost host = Host.CreateDefaultBuilder(args).
                ConfigureServices(services => {
                    services.AddSingleton(typeof(Serilog.ILogger), _logger);
                    services.AddHostedService<Worker>();
                }).Build();

            host.Run();

            Console.ReadKey();
        }
    }
}