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
        public Task<int>? ActivityStartTask { get; set; }

        public System.Diagnostics.Process? Process { get; set; }

        public ActivityStartInfo(Task<int> activityStartTask, System.Diagnostics.Process process, CommonOperationResult result) 
        { 
            ActivityStartTask = activityStartTask;
            Process = process;
            Result = result;
        }
        public CommonOperationResult Result { get; set; }

    }
}
