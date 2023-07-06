using ActivityScheduler.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public class ActivityStartInfo
    {
        public Thread? ActivityStartThread { get; set; }

        public System.Diagnostics.Process? Process { get; set; }

        public ActivityStartInfo(Thread? activityStartThread, System.Diagnostics.Process process, CommonOperationResult result) 
        {
            ActivityStartThread = activityStartThread;
            Process = process;
            Result = result;

        }
        public CommonOperationResult Result { get; set; }

    }
}
