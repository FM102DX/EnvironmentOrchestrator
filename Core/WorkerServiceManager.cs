using ActivityScheduler.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace ActivityScheduler.Core.Appilcation
{
    public class WorkerServiceManager
    {

        private Serilog.ILogger _logger;

        private ActivitySchedulerApp _app;
        public WorkerServiceManager(ActivitySchedulerApp app, Serilog.ILogger logger)
        {
            _logger = logger;
            _app = app;
        }

        public bool DoesServiceExist(string serviceName, string machineName = "localhost")
        {
            ServiceController[] services = ServiceController.GetServices(machineName);
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
        }

        public CommonOperationResult InstallService()
        {
            if (DoesServiceExist(_app.WinServiceName))
            {
                return CommonOperationResult.SayOk();
            }
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo.FileName = _app.ServiceExeFileFullPath;
                startInfo.Arguments = " install";
                process.StartInfo = startInfo;
                process.Start();

                _logger.Information($"Installed service, waiting for state Stopped");
                bool canProceed = false;
                bool installSuccessful = false;
                int count = 0;
                do
                {
                    var stt = GetServiceState();
                    installSuccessful = stt == ServiceSateEnum.Stopped;
                    if (!installSuccessful)
                    {
                        Thread.Sleep(1000);
                        count++;
                        if (count > 20) { canProceed = true; }
                        _logger.Information($"Waiting for state Stopped, tries={count}, current state={stt.ToString()} ");
                    }
                    else
                    {
                        canProceed = true;
                    }
                }
                while (!canProceed);

                if (!installSuccessful)
                {
                    throw new Exception();
                }
                return CommonOperationResult.SayOk();
            }
            catch
            {
                string msg = "Failed to start install service. Try to run this app from administrator";
                _logger.Information(msg);
                return CommonOperationResult.SayFail(msg);
            }
        }

        public void UninstallService()
        {
            if (!DoesServiceExist(_app.WinServiceName)) return;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = _app.ServiceExeFileFullPath;
            startInfo.Arguments = " uninstall";
            process.StartInfo = startInfo;
            process.Start();
        }

        public ServiceSateEnum GetServiceState()
        {
            ServiceController[] services = ServiceController.GetServices("localhost");
            var service = services.FirstOrDefault(s => s.ServiceName == _app.WinServiceName);
            if (service == null) { return ServiceSateEnum.NotInstalled; }
            if (service.Status.ToString() == "Running") { return ServiceSateEnum.Running; }
            return ServiceSateEnum.Stopped;
        }

        public CommonOperationResult StartService()
        {
            ServiceSateEnum stt = GetServiceState();
            if (stt == ServiceSateEnum.Running) { return CommonOperationResult.SayOk(); }
            if (stt == ServiceSateEnum.Stopped)
            {
                ServiceController[] services = ServiceController.GetServices("localhost");
                var service = services.FirstOrDefault(s => s.ServiceName == _app.WinServiceName);
                try
                {
                    service.Start();
                    _logger.Information($"Started service, waiting for state Running");
                    bool canProceed = false;
                    bool startSuccessful = false;
                    int count = 0;
                    do
                    {
                        stt = GetServiceState();
                        startSuccessful = stt == ServiceSateEnum.Running;
                        if (!startSuccessful)
                        {
                            Thread.Sleep(1000);
                            count++;
                            if (count > 20) { canProceed = true; }
                            _logger.Information($"Waiting for state Running, tries={count}, current state={stt.ToString()} ");
                        }
                        else
                        {
                            canProceed = true;
                        }
                    }
                    while (!canProceed);

                    if (!startSuccessful)
                    {
                        throw new Exception();
                    }
                    return CommonOperationResult.SayOk();
                }
                catch (Exception ex)
                {
                    string msg = $"Failed to start worker service. Try to run this app from administrator. Exception: {ex.Message}, innerexception {ex.InnerException}";
                    _logger.Information(msg);
                    return CommonOperationResult.SayFail(msg);
                }
            }
            else
            {
                string msg = $"Failed to start worker service {_app.WinServiceName} because it is in state {stt.ToString()}";
                _logger.Error(msg);
                return CommonOperationResult.SayFail(msg);
            }
        }

        public CommonOperationResult StopService()
        {
            ServiceSateEnum stt = GetServiceState();
            if (stt == ServiceSateEnum.Running)
            {
                ServiceController[] services = ServiceController.GetServices("localhost");
                var service = services.FirstOrDefault(s => s.ServiceName == _app.WinServiceName);
                try
                {
                    service.Stop();
                    _logger.Information($"Stopped service, waiting for state Stopped");
                    bool canProceed = false;
                    bool startSuccessful = false;
                    int count = 0;
                    do
                    {
                        stt = GetServiceState();
                        startSuccessful = stt == ServiceSateEnum.Stopped;
                        if (!startSuccessful)
                        {
                            Thread.Sleep(1000);
                            count++;
                            if (count > 20) { canProceed = true; }
                            _logger.Information($"Waiting for state Stopped, tries={count}, current state={stt.ToString()} ");
                        }
                        else
                        {
                            canProceed = true;
                        }
                    }
                    while (!canProceed);

                    if (!startSuccessful)
                    {
                        throw new Exception();
                    }
                    return CommonOperationResult.SayOk();
                }
                catch
                {
                    string msg = "Failed to stop worker service. Try to run this app from administrator";
                    _logger.Information(msg);
                    return CommonOperationResult.SayFail(msg);
                }
            }
            else
            {
                string msg = $"Failed to stop worker service {_app.WinServiceName} because it is in state {stt.ToString()}";
                _logger.Error(msg);
                return CommonOperationResult.SayFail(msg);
            }
        }

        public enum ServiceSateEnum
        {
            NotInstalled = 1,
            Running = 2,
            Stopped = 3
        }
    }
}
