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

            try
            {
                var exitCode = HostFactory.Run(x =>
                {
                    x.Service<Worker>(s =>
                    {
                        s.ConstructUsing(service => new Worker(_logger, app));
                        s.WhenStarted(service => service.Start());
                        s.WhenStopped(service => service.Stop());
                    });
                    x.RunAsLocalSystem();
                    x.SetServiceName(app.WinServiceName);
                    x.SetDisplayName(app.WinServiceDiaplayName);
                    x.SetDescription(app.WinServiceDescription);
                });
                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                Environment.ExitCode = exitCodeValue;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }
    }
}