using ActivityScheduler.Shared;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Topshelf;

namespace ActivityScheduler.WorkerService.TopShelf
{
    internal class Program
    {
        static void Main(string[] args)
        {
            

            ActivitySchedulerWorkerApp app = new ActivitySchedulerWorkerApp();

            string logFilePath = System.IO.Path.Combine(app.LogsDirectory, Functions.GetNextFreeFileName(app.LogsDirectory, "ActivitySchedulerLogs", "txt"));

            Serilog.ILogger _logger = new LoggerConfiguration()
                  .WriteTo.File(logFilePath)
                  .CreateLogger();

            _logger.Information("This is Activity scheduler worker service starting");

            try
            {
                var exitCode = HostFactory.Run(x =>
                {
                    x.Service<WorkerContainer>(s =>
                    {
                        s.ConstructUsing(service => new WorkerContainer(_logger, app));
                        s.WhenStarted(service => service.Start(null));
                        s.WhenStopped(service => service.Stop(null));
                    });
                    // x.RunAsLocalSystem();
                    x.RunAs("HOMECOM01\\Admin", "123");
                    x.SetServiceName(app.WinServiceName);
                    x.SetDisplayName(app.WinServiceDiaplayName);
                    x.SetDescription(app.WinServiceDescription);
                    x.StartAutomatically();
                    
                    _logger.Information("Completed setup");
                });
                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                Environment.ExitCode = exitCodeValue;
                _logger.Information($"Exit with code {exitCodeValue}") ;
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to start with exception = {e.Message}, innerexception = {e.InnerException}");
                Environment.Exit(-1);
            }
        }
    }
}