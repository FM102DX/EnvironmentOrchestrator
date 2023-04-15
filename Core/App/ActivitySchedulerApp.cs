using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Core.Appilcation
{
    public class ActivitySchedulerApp
    {

        public ActivitySchedulerApp() { }

        public String? BaseDirectory { get => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }

        public String? LogsDirectory
        {
            get
            {
                var directory = System.IO.Path.Combine(BaseDirectory, "Logs");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }
        public String? DataDirectory
        {
            get
            {
                var directory = System.IO.Path.Combine(BaseDirectory, "Data");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public String? OutputDirectory
        {
            get
            {
                var directory = System.IO.Path.Combine(BaseDirectory, "Output");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public String? IconsDirectory
        {
            get
            {
                var directory = System.IO.Path.Combine(BaseDirectory, "Assets","Icons");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }
        
        public String? ServiceExeFileFullPath
        {
            get
            {
                var directory = "C:\\Develop\\ActivityScheduler\\ActivityScheduler.WorkerService.TopShelf\\bin\\Debug\\net6.0\\ActivityScheduler.WorkerService.TopShelf.exe";
                return directory;
            }
        }


        public String? WinServiceName => "A01";
        public String? WinServiceDiaplayName => "A01_Display_Name";
        public String? WinServiceDescription => "A01_Description";

        public bool DoesServiceExist(string serviceName, string machineName = "localhost")
        {
            ServiceController[] services = ServiceController.GetServices(machineName);
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
        }

        public void InstallService()
        {
            if (DoesServiceExist(WinServiceName)) return;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = ServiceExeFileFullPath;
            startInfo.Arguments = " install";
            process.StartInfo = startInfo;
            process.Start();
        }
        public void UninstallService()
        {
            if (!DoesServiceExist(WinServiceName)) return;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = ServiceExeFileFullPath;
            startInfo.Arguments = " uninstall";
            process.StartInfo = startInfo;
            process.Start();
        }

        public string GetServiceState()
        {
            ServiceController[] services = ServiceController.GetServices("localhost");
            var service = services.FirstOrDefault(s => s.ServiceName == WinServiceName);
            if (service ==null) { return "Not installed"; }
            return service.Status.ToString();
        }

        public void StartService()
        {
            ServiceController[] services = ServiceController.GetServices("localhost");
            var service = services.FirstOrDefault(s => s.ServiceName == WinServiceName);
            if (service != null) { service.Start();  }
        }

        public void StopService()
        {
            ServiceController[] services = ServiceController.GetServices("localhost");
            var service = services.FirstOrDefault(s => s.ServiceName == WinServiceName);
            if (service != null) { service.Stop(); }
        }
    }
}
