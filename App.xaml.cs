using ActivityScheduler.Core.Appilcation;
using ActivityScheduler.Core.Settings;
using ActivityScheduler.DataAccess;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Pipes;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;


namespace ActivityScheduler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ServiceProvider serviceProvider;
        private Serilog.ILogger Logger;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private MainWindow mainWindow;
        private ActivityScheduler.Core.TrayContextMenu trayContextMenu;
        private ServiceProvider _serviceProvider;
        private EfAsyncRepository<SettingStorageUnit> settingsRepo;
        private ActivitySchedulerApp _app;
        private WorkerServiceManager _workerMgr;
        private PipeServerHelper _serverPipe;
        private PipeListener _pipeClient;
        private readonly System.Timers.Timer _timer;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            //timer 1
            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += SendPipeMessage;
        }
        private void SendPipeMessage(object? sender, ElapsedEventArgs e)
        {
            Random random = new Random();
            int x = random.Next(0, 1000);
            string msg = $"Pipe server is sending message {x} to Pipe02";
            _serverPipe.SendMessage(msg);
            Logger.Information(msg);
        }
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();

            services.AddSingleton(typeof(ActivitySchedulerApp), (x) => new ActivitySchedulerApp());

            services.AddSingleton<WorkerServiceManager>();

            var _serviceProvider = services.BuildServiceProvider();

            _app = _serviceProvider.GetService<ActivitySchedulerApp>();

            // Functions.LeaveLastNFilesOrFoldersInDirectory(_app.LogsDirectory, 20);

            string logFilePath= System.IO.Path.Combine(_app.LogsDirectory, Functions.GetNextFreeFileName(_app.LogsDirectory, "ActivitySchedulerLogs","txt")); 

            Serilog.ILogger _logger = new LoggerConfiguration()
                  .WriteTo.File(logFilePath)
                  .CreateLogger();

            Process[] pname = Process.GetProcessesByName("ActivityScheduler");
            _logger.Information($"Intances of ActivityScheduler.exe {pname.Length}");
            if (pname.Length > 1)
            {
                System.Windows.MessageBox.Show("Only one instance of application can be running");
                Shutdown();
            }

            services.AddSingleton(typeof(Serilog.ILogger), (x) => _logger);

            EFSqliteDbContext sqLiteDbContext = new EFSqliteDbContext(_app.DataDirectory);
            
            sqLiteDbContext.Database.EnsureCreated();
            _logger.Information($"Point 1");
            try
            {
                settingsRepo = new EfAsyncRepository<SettingStorageUnit>(sqLiteDbContext);
                services.AddSingleton(typeof(IAsyncRepositoryT<SettingStorageUnit>), (x) => settingsRepo);

                /*
                 * 
                    employeesRepo = new EfAsyncRepository<Employee>(sqLitedbContext);
                    rolesRepo = new EfAsyncRepository<EmployeeRole>(sqLitedbContext);
                    messagesRepo = new EfAsyncRepository<ConsoleToApiMessage>(sqLitedbContext);
                    employeesRepo.InitAsync(true);

                    Log.Logger.Information($"SQLITE repos  initialized ok");

                    services.AddTransient(typeof(IAsyncRepositoryT<Employee>), (x) => new EfAsyncRepository<Employee>(new EFSqliteDbContext()));
                    services.AddTransient(typeof(IAsyncRepositoryT<EmployeeRole>), (x) => new EfAsyncRepository<EmployeeRole>(new EFSqliteDbContext()));

                    services.AddScoped(typeof(IAsyncRepositoryT<Employee>), (x) => new EfAsyncRepository<Employee>(new EFSqliteDbContext()));
                    services.AddScoped(typeof(IAsyncRepositoryT<EmployeeRole>), (x) => new EfAsyncRepository<EmployeeRole>(new EFSqliteDbContext()));
                    services.AddScoped(typeof(IAsyncRepositoryT<ConsoleToApiMessage>), (x) => new EfAsyncRepository<ConsoleToApiMessage>(new EFSqliteDbContext()));

                   */

            }
            catch (Exception ex)
            {
                 _logger.Error($"ERROR while registering repositories: message={ex.Message} innerexception={ex.InnerException}");
            }
            _logger.Information($"Point 2");
            services.AddSingleton<SettingsManager>();

        }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            
            var app = _serviceProvider.GetService<ActivitySchedulerApp>();

            string iconFileFullPath = Path.Combine(app.IconsDirectory, "app.ico");

            notifyIcon1 = new NotifyIcon();

            notifyIcon1.Icon = new System.Drawing.Icon(iconFileFullPath);

            notifyIcon1.Text = "Activity Scheduler";

            notifyIcon1.Visible = true;

            notifyIcon1.MouseDown += NotifyIcon1_MouseDown;

            notifyIcon1.MouseDoubleClick += NotifyIcon1_MouseDoubleClick;

            SettingsManager mgr = _serviceProvider.GetService<SettingsManager>();

            mainWindow = _serviceProvider.GetService<MainWindow>();

            var _logger = _serviceProvider.GetService<Serilog.ILogger>();

            Logger = _logger;

            Logger.Information("Starting ActivityScheduler app");

            var icon = new Icon(SystemIcons.Exclamation, 40, 40);
            
            _logger.Information($"Point 4");
            
            trayContextMenu = new ActivityScheduler.Core.TrayContextMenu(this);
            
            _logger.Information($"Point 5");

            //install and run worker service

            _workerMgr = _serviceProvider.GetService<WorkerServiceManager>();

            _logger.Information($"Point 6");

            CommonOperationResult installResult = _workerMgr.InstallService();

            if (!installResult.Success)
            {
                System.Windows.MessageBox.Show(installResult.Message);
                throw new Exception(installResult.Message);
            }
            _logger.Information($"Point 7");

            CommonOperationResult startResult = _workerMgr.StartService();
            
            if (!startResult.Success)
            {
                System.Windows.MessageBox.Show(startResult.Message);
                throw new Exception(startResult.Message);
            }
            _logger.Information($"Point 8");


            _serverPipe = new PipeServerHelper("Pipe02");
            _serverPipe.Run();
            _timer.Start();

            _pipeClient = new PipeListener("Pipe01", _logger);
            Task task = Task.Run(() => _pipeClient.Run());  
            

            

            mainWindow.Show();
            _logger.Information($"Point 9");
        }

        private void NotifyIcon1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            ShowMainWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon1.Visible = false;
            base.OnExit(e);
        }

        private void NotifyIcon1_MouseDown(object? sender, MouseEventArgs e)
        {
            
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu menu = (ContextMenu)trayContextMenu.FindResource("NotifierContextMenu");

                menu.IsOpen = true;

                Logger.Information("Context menu opened by r-click");
            }
        }

        public void ShowMainWindow()
        {
            if(mainWindow.Tag=="Closed")
            {
                mainWindow = new MainWindow(_serviceProvider.GetService<SettingsManager>(), _app, _workerMgr);
            }
            
            mainWindow.WindowState= WindowState.Normal;
            mainWindow.Show();


        }
        public void HideMainWindow()
        {
            mainWindow.Hide();
        }
    }
}
