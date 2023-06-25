using ActivityScheduler.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public  class Activity: BaseEntity
    {
        public Guid BatchId { get; set; }

        public Batch? ParentBatch { get; set; }

        public string Name { get; set; }

        public int ActivityId { get; set; } // number like 10, 20, 30 ... 450, 460, ets 
        
        public TimeSpan StartTime { get; set; }

        public String TransactionId { get; set; } //number of transaction in this script / foreign script

        public bool IsDomestic { get; set; } //wether domestic transaction should be called
        
        public bool IsHub { get; set; } //wether this transaction is a hub (hubs do nothing but become successful to start child activities)

        public TimeSpan ChildDelay { get; set; } // starttime that's passed to another script as parameter, is for ChildDelay more than fact starttime

        public bool AlwaysSuccess { get; set; }
        
        public string? ParentActivities { get; set; }
        
        public string? ScriptPath { get; set; }

        public bool IsActive { get; set; } = true;

        public ActivityStatusEnum Status { get; set; }

        public int RetriesAvivble { get; set; }

        public int RetriesPerformed { get; set; }

        public List<int> GetParentActionIds()
        {
            if (string.IsNullOrEmpty(ParentActivities)) { return new List<int>(); }
            
            var x = ParentActivities.Split(',').ToList().Select(x=>Convert.ToInt32(x)).ToList();
            
            return x;
        }

        public ActivityParentRuleEnum ActivityParentRule { get; set; }

        public ActivityGridViewModel AsViewModel()
        {
            ActivityGridViewModel rez= new ActivityGridViewModel();
            rez.Id = Id;
            rez.Name = Name;
            rez.ActivityId = ActivityId;
            rez.AlwaysSuccess = AlwaysSuccess;
            rez.StartTime = StartTime;
            rez.TransactionId = TransactionId;
            rez.ChildDelay = ChildDelay;
            rez.IsDomestic = IsDomestic;
            rez.IsHub = IsHub;
            rez.IsActive = IsActive;

            rez.ParentActivities = ParentActivities;
            return rez;
        }

        public bool IsTimeDriven
        {
            get => GetParentActionIds().Count == 0;
        }
        public bool IsParentDriven
        {
            get => !IsTimeDriven;
        }

        public bool IsWaitingOrIdle
        {
            get=>Status== ActivityStatusEnum.Waiting || Status== ActivityStatusEnum.Idle;
        }

        public void Run (DateTime activityStartDateTime, string workingFolder, string jobName)
        {
            
        }

        public string ScriptPathCalculated
        {
            get
            {
                string? s = "";

                if(ParentBatch != null)
                {
                    s = ParentBatch.DefaultScriptPath;
                }

                if (!string.IsNullOrEmpty(ScriptPath))
                {
                    s = ScriptPath;
                }

                return (string.IsNullOrEmpty(s)) ? "" : s;
            }
        }
        public ActivityStartInfo Run()
        {
            //run powershell with params
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            var rezTask = Task<int>.Run(() => {
                string appName = "powershell.exe";
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo.FileName = appName;
                startInfo.Arguments = $"-file {ScriptPath} -transactionId {TransactionId}";
                process.StartInfo = startInfo;
                process.Start();
                return 1;
            });
            return new ActivityStartInfo(rezTask, process);
        }
    }
}
