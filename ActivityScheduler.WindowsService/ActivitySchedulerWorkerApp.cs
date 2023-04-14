using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.WorkerService
{
    public class ActivitySchedulerWorkerApp
    {

        public ActivitySchedulerWorkerApp() { }

        public String? BaseDirectory { get => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }

        public String? LogsDirectory
        {
            get
            {
                var directory = System.IO.Path.Combine(BaseDirectory, "LogsWorker");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }
 

    }
}
