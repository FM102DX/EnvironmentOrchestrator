using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public class Worker
    {
        private readonly System.Timers.Timer _timer;
        private readonly System.Timers.Timer _timer2;
        private Serilog.ILogger _logger;
        private ActivitySchedulerWorkerApp _app;
        private CancelToken _token;
        public Worker(Serilog.ILogger logger, ActivitySchedulerWorkerApp app)
        {
            _logger = logger;
            _logger.Information("Workes service business logic class constructor");
            _timer2 = new System.Timers.Timer(1000) { AutoReset = true };
            _timer2.Elapsed += ExecuteEvent2;
            _app= app;
            _logger.Information("Workes service business logic class constructor--passed ok");
        }

        private void ExecuteEvent2(object? sender, ElapsedEventArgs e)
        {
            _logger.Information("This is topshelf worker teak");
        }

        private void CheckMainAppRunning(CancelToken token)
        {
            do
            {
                Process[] pname = Process.GetProcessesByName(_app.MainAppProcessName);
                _logger.Information($"Doing main loop, pname.Length={pname.Length}");
                if (pname.Length == 0)
                {
                    //main app not running, need to start
                    _logger.Information("Main app not running, need to start");
                    try
                    {
                        System.Security.SecureString ssPwd = new System.Security.SecureString();
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                        startInfo.CreateNoWindow = false;
                        startInfo.FileName = _app.MainAppDirectory;
                        startInfo.Arguments = "";
                        _logger.Information("...configured data");
                        startInfo.UserName = "Admin";
                        string password = "123";
                        for (int x = 0; x < password.Length; x++)
                        {
                            ssPwd.AppendChar(password[x]);
                        }
                        password = "";
                        startInfo.Password = ssPwd;
                        _logger.Information("...configured creds");
                        process.StartInfo = startInfo;
                        process.Start();

                        _logger.Information("Started main app, waiting for it to be in running state");
                        Thread.Sleep(5000);
                        pname = Process.GetProcessesByName(_app.MainAppProcessName);
                        if (pname.Length == 0)
                        {
                            _logger.Error("Failed to start main app");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to start main app with exception = {ex.Message}, innerexception = {ex.InnerException}");
                    }

                }
                Thread.Sleep(1000);
            }
            while (!token.Cancelled);
        }

        public void Stop()
        {
            _timer2.Stop();
            _token.Cancel();
        }
        public void Start()
        {
            _timer2.Start();
            _token = new CancelToken();
            CheckMainAppRunning(_token);
        }

        private class CancelToken
        {
            public bool Cancelled { get; set; } = false;
            public void Cancel() { Cancelled = true; }
        }

    }
}
