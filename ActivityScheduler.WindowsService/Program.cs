using ActivityScheduler.Shared;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Drawing;
using ActivityScheduler.WorkerService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.EventLog;

namespace ActivityScheduler.WindowsService
{
    public class Program
    {
        static void Main(string[] args)
        {

            ActivitySchedulerWorkerApp app = new ActivitySchedulerWorkerApp();
            var y = app.ExeFileFullPath;
            string logFilePath = System.IO.Path.Combine(app.LogsDirectory, Functions.GetNextFreeFileName(app.LogsDirectory, "ActivitySchedulerLogs", "txt"));

            Serilog.ILogger _logger = new LoggerConfiguration()
                  .WriteTo.File(logFilePath)
                  .CreateLogger();

            IHost host = Host.CreateDefaultBuilder(args).
                ConfigureServices(services => {
                    services.AddSingleton(typeof(Serilog.ILogger), _logger);
                    services.AddHostedService<Worker>();

                    if(OperatingSystem.IsWindows())
                    {
                        services.Configure<EventLogSettings>(config =>
                        {
                            if (OperatingSystem.IsWindows())
                            {
                                config.LogName = "SampleService";
                                config.SourceName = "SampleServiceName";
                            };
                        });
                    };
                }).Build();

            host.Run();

            Console.ReadKey();
        }
    }
}