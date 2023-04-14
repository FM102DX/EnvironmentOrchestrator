using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    }
}
