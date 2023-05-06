using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ActivityScheduler.Shared.Pipes;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public class Worker
    {
        private readonly System.Timers.Timer _timer;
        private readonly System.Timers.Timer _timer2;
        private Serilog.ILogger _logger;
        private ActivitySchedulerWorkerApp _app;
        private CancelToken _token;
        private ClientCommunicationObjectT<AppToWorkerMessage> _pipeClient;
        private ServerCommunicationObjectT<WorkerToAppMessage> _pipeServer;
        public Worker(Serilog.ILogger logger, ActivitySchedulerWorkerApp app)
        {
            _logger = logger;
            _logger.Information("Workes service business logic class constructor");

            //timer 1
            _timer = new System.Timers.Timer(500) { AutoReset = true };
            _timer.Elapsed += SendPipeMessage;

            //timer 2
            //_timer2 = new System.Timers.Timer(500) { AutoReset = true };
            //_timer2.Elapsed += ExecuteEvent2;
            _app= app;

            _logger.Information("Workes service business logic class constructor--passed ok");

            _pipeClient = new ClientCommunicationObjectT<AppToWorkerMessage>("Pipe02", _logger);
            Task task = Task.Run(() => _pipeClient.Run());

            _pipeServer = new ServerCommunicationObjectT<WorkerToAppMessage>("Pipe01", _logger);
            Task task2 = Task.Run(() => _pipeServer.Run());

        }

        private void ExecuteEvent2(object? sender, ElapsedEventArgs e)
        {
            //_logger.Information("This is topshelf worker teak");
        }

        private void SendPipeMessage(object? sender, ElapsedEventArgs e)
        {
            Random random = new Random();
            int x = random.Next(0, 1000);
            string msg = $"Pipe server is sending message {x} to {_pipeServer.PipeName} ";

            var msgObject = new WorkerToAppMessage()
            {
                Message = msg,
                Command="ping",
                Result = Shared.CommonOperationResult.SayOk(msg)
            };

            _pipeServer.SendObject(msgObject);
            _logger.Information($"Worker service is sending message: {msg}");
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
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = false;
                        //startInfo.FileName = _app.MainAppDirectory;
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = $"/C {_app.MainAppDirectory}";

                        //startInfo.FileName = "C:\\Develop\\Bats\\runwpf.bat";
                        //_logger.Information("...configured data");
                        
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
                        bool rez=process.Start();
                        _logger.Error($"process.Start={rez}");
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
            //_timer2.Stop();
            _timer.Stop();
            //_token.Cancel();
        }
        public void Start()
        {
            //_timer2.Start();
            _timer.Start();
            //_token = new CancelToken();
            //CheckMainAppRunning(_token);
        }

        private class CancelToken
        {
            public bool Cancelled { get; set; } = false;
            public void Cancel() { Cancelled = true; }
        }

    }
}
