using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ActivityScheduler.WorkerService
{
    public class ActivitySchedulerWorkerApp
    {

        public ActivitySchedulerWorkerApp() { }

        public String? BaseDirectory { get => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }

        public String? MainAppDirectory { get => "C:\\Develop\\WinFormsApp1\\bin\\Debug\\net6.0-windows\\WinFormsApp1.exe"; }

        public String? MainAppProcessName { get => "WinFormsApp1"; }

        public String? LogsDirectory
        {
            get
            {
                var directory = System.IO.Path.Combine(BaseDirectory, "LogsWorker");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public String? ExeFileFullPath => Process.GetCurrentProcess().MainModule.FileName;
        
        public String? WinServiceName => "A01";
        public String? WinServiceDiaplayName => "A01_Display_Name";
        public String? WinServiceDescription => "A01_Description";
        public String? DataDirectory { get => "C:\\Develop\\ActivityScheduler\\bin\\Debug\\net6.0-windows\\Data"; }


    }
}
