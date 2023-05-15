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
    public class ActivitySchedulerApp
    {

        public ActivitySchedulerApp() 
        {
            
        }

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

        public string PlayIconFullPath => Path.Combine(IconsDirectory, "PlayIcon.jpg");
        public string NoneIconFullPath => Path.Combine(IconsDirectory, "NoneIcon.jpg");
    }
}
