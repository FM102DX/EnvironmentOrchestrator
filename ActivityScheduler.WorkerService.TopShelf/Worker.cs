using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.DataAccess;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Data.Models.Settings;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Pipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public class Worker
    {
        private readonly System.Timers.Timer _timer;
        private readonly System.Timers.Timer _checkMailTimer;
        private Serilog.ILogger _logger;
        private ActivitySchedulerWorkerApp _app;
        private CancelToken _token;
        private ClientCommunicationObjectT<AppToWorkerMessage> _pipeClient;
        private ServerCommunicationObjectT<WorkerToAppMessage> _pipeServer;
        private BatchRunner _batchRunner;
        private BatchManager _batchManager;
        private ActivityManager _activityManager;
        private ServiceProvider _serviceProvider;
        public Worker(Serilog.ILogger logger, ActivitySchedulerWorkerApp app)
        {

            _logger = logger;
            _logger.Information("Workes service business logic class constructor");

            //timer 1
            _timer = new System.Timers.Timer(500) { AutoReset = true };
            _timer.Elapsed += SendPipeMessage;

            //_checkMailTimer 2
            _checkMailTimer = new System.Timers.Timer(500) { AutoReset = true };
            _checkMailTimer.Elapsed += CheckMail;
            _app = app;

            _pipeClient = new ClientCommunicationObjectT<AppToWorkerMessage>("Pipe02", _logger);
            Task task = Task.Run(() => _pipeClient.Run());

            _pipeServer = new ServerCommunicationObjectT<WorkerToAppMessage>("Pipe01", _logger);
            Task task2 = Task.Run(() => _pipeServer.Run());

            _logger.Information("Worker service constructor passed");

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            _logger.Information("Worker service ConfigureServices passed");
        }

        private void ConfigureServices(ServiceCollection services)
        {
            _logger.Information("entered ConfigureServices");

            services.AddSingleton(typeof(Serilog.ILogger), (x) => _logger);

            EFSqliteDbContext sqLiteDbContext = new EFSqliteDbContext(_app.DataDirectory);
            _logger.Information("P1");
            sqLiteDbContext.Database.EnsureCreated();
            _logger.Information("P2");
            
            try
            {
                services.AddSingleton(typeof(IAsyncRepositoryT<SettingStorageUnit>), (x) => new EfAsyncRepository<SettingStorageUnit>(sqLiteDbContext));
                services.AddSingleton(typeof(IAsyncRepositoryT<Data.Models.Activity>), (x) => new EfAsyncRepository<Data.Models.Activity>(sqLiteDbContext));
                services.AddSingleton(typeof(IAsyncRepositoryT<Batch>), (x) => new EfAsyncRepository<Batch>(sqLiteDbContext));
            }
            catch (Exception ex)
            {
                _logger.Error($"ERROR while registering repositories: message={ex.Message} innerexception={ex.InnerException}");
            }

            _logger.Information($"Point 2");
            _logger.Information("P3");
            services.AddSingleton<SettingsManager>();
            services.AddSingleton<ActivityManager>();
            services.AddSingleton<BatchManager>();
            services.AddSingleton<BatchRunner>();
            
            _logger.Information("P4");
        }
        private void CheckMail(object? sender, ElapsedEventArgs e)
        {
            _logger.Information($"listening to incoming stack");
            Data.Models.Communication.AppToWorkerMessage? m = _pipeClient.Take();
            var btcr = _serviceProvider.GetService<BatchRunner>();

            if (m == null) 
            { 
                _logger.Information($"got null");
                return; 
            }

            _logger.Information($"Got message, MessageType={m.MessageType} Command={m.Command.ToLower()}");

            if(m.MessageType.ToLower()=="Command".ToLower())
            {
                if (m.Command.ToLower() == "startbatch")
                {
                    _logger.Information($"got message of startbatch type");
                    Task.Run(()=> {
                        var rez=btcr.RunBatch(m.TransactionId);
                        var msgObject = new WorkerToAppMessage()
                        {
                            MessageType = "CommandExecutionResult".ToLower(),
                            Result=rez
                        };
                        _pipeServer.SendObject(msgObject);
                    });
                }
            }
            Task.Delay(100);
        }

        private void SendPipeMessage(object? sender, ElapsedEventArgs e)
        {
            Random random = new Random();
            var btcr = _serviceProvider.GetService<BatchRunner>();
            //int x = random.Next(0, 1000);
            //string msg = $"Pipe server is sending message {x} to {_pipeServer.PipeName} ";

            var msgObject = new WorkerToAppMessage()
            {
                MessageType = "runningbatchesInfo",
                RunningBatches = btcr.GetRunningBatchesInfo()
            };
            _pipeServer.SendObject(msgObject);
        }

        public void Stop()
        {
            
            _timer.Stop();
            _checkMailTimer.Stop();
        }
        public void Start()
        {
            
            _timer.Start();
            _checkMailTimer.Start();
        }

        private class CancelToken
        {
            public bool Cancelled { get; set; } = false;
            public void Cancel() { Cancelled = true; }
        }

    }
}
