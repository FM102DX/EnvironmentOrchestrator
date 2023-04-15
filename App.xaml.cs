﻿using ActivityScheduler.Core.Appilcation;
using ActivityScheduler.Core.Settings;
using ActivityScheduler.DataAccess;
using ActivityScheduler.Shared;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;


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
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();

            services.AddSingleton(typeof(ActivitySchedulerApp), (x) => new ActivitySchedulerApp());

            var _serviceProvider = services.BuildServiceProvider();

            _app= _serviceProvider.GetService<ActivitySchedulerApp>();

            Functions.LeaveLastNFilesOrFoldersInDirectory(_app.LogsDirectory, 20);

            string logFilePath= System.IO.Path.Combine(_app.LogsDirectory, Functions.GetNextFreeFileName(_app.LogsDirectory, "ActivitySchedulerLogs","txt")); 

            Serilog.ILogger _logger = new LoggerConfiguration()
                  .WriteTo.File(logFilePath)
                  .CreateLogger();

            services.AddSingleton(typeof(Serilog.ILogger), (x) => _logger);

            EFSqliteDbContext sqLiteDbContext = new EFSqliteDbContext(_app.DataDirectory);
            
            sqLiteDbContext.Database.EnsureCreated();

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

            trayContextMenu = new ActivityScheduler.Core.TrayContextMenu(this);

            if(!app.DoesServiceExist(app.WinServiceName))
            {
                app.InstallService();
            }

            if (!app.DoesServiceExist(app.WinServiceName))
            {
                String msg = "Failed to install worker service. Try to run this app from administrator.";
                System.Windows.MessageBox.Show(msg);
                Logger.Information(msg);
                throw new Exception(msg);
            }
            Logger.Information("Starting service");
            app.StartService();
            
            bool canProceed=false;
            bool startSuccessful = false;
            int count = 0;
            do
            {
                string stt = app.GetServiceState();
                startSuccessful = stt == "Running";
                Logger.Information($"stt={stt}");
                if (!startSuccessful)
                {
                    Thread.Sleep(1000);
                    count++;
                    if (count>20) { canProceed = true; }
                    Logger.Information($"Start unsuccessful, count={count}");
                }
                else
                {
                    canProceed = true;
                }
            }
            while (!canProceed);

            Logger.Information($"End of wait loop");

            if (!startSuccessful)
            {
                string stt = app.GetServiceState();
                Logger.Information($"State={stt}");
                string msg = "Failed to start worker service. Try to run this app from administrator.";
                System.Windows.MessageBox.Show(msg);
                Logger.Information(msg);
                throw new Exception(msg);
            }

    

            mainWindow.Show();
        }

        private void NotifyIcon1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            mainWindow = new MainWindow(_serviceProvider.GetService<SettingsManager>(), _app);

            mainWindow.Show();
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
            mainWindow = new MainWindow(_serviceProvider.GetService<SettingsManager>(), _app);

            mainWindow.Show();
            
        }
        public void HideMainWindow()
        {
            mainWindow.Hide();
        }
    }
}
