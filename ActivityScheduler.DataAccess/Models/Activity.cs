﻿using ActivityScheduler.Data.DataAccess;
using ActivityScheduler.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ActivityScheduler.Shared;

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

        public ActivityStatusEnum Status { get; set; } = ActivityStatusEnum.Idle;

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

        [NotMapped]
        public Stopwatch _stopwatch = new Stopwatch();


        [NotMapped]
        public int ActivityRunResult { get; set; }

        [NotMapped]
        public TimeSpan TimeLeftToStart { get; set; }

        //total elapsed time considering retries
        [NotMapped]
        public string TimeLeftToStartAsString => Functions.TimeSpanAsString(TimeLeftToStart);

        //total elapsed time considering retries
        [NotMapped]
        public TimeSpan ElapsedTime { get; set; }

        [NotMapped]
        public string ElapsedTimeAsString => Functions.TimeSpanAsString(ElapsedTime);

        [NotMapped]
        private System.Timers.Timer _timer;

        public bool IsWaitingOrIdle
        {
            get=>Status== ActivityStatusEnum.Waiting || Status== ActivityStatusEnum.Idle;
        }

        public ActivityStartInfo Run(Serilog.ILogger logger, string? scriptPath)
        {
            string errText;

            if (string.IsNullOrEmpty(scriptPath))
            {
                errText = $"Activity: no script path scpecified when trying to run activity {ActivityId}";
                logger.Error(errText);
                return new ActivityStartInfo(null, null, CommonOperationResult.SayFail(errText));
            }
            try
            {
                //run powershell with params
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                ActivityRunResult = 0;
                var rezThread = new Thread(() => {
                   
                    _timer = new System.Timers.Timer(500);
                    _timer.AutoReset = true;
                    _timer.Elapsed += (s, e) => { ElapsedTime = _stopwatch.Elapsed; };
                    _timer.Start();
                    _stopwatch.Start();

                    logger.Information($"[Activity.Run] -- running activity {ActivityId}, -file {scriptPath} -transactionId {TransactionId} _stopwatch {_stopwatch.Elapsed}");
                    string appName = "powershell.exe";
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    startInfo.FileName = appName;
                    startInfo.Arguments = $"-file {scriptPath} -transactionId {TransactionId}";
                    process.StartInfo = startInfo;
                    
                    process.Start();
                    logger.Information($"[Activity.Run] started process, waiting for exit");
                    process.WaitForExit();
                    logger.Information($"[Activity.Run] exited with code {process.ExitCode} _stopwatch {_stopwatch.Elapsed}");
                    bool runRez = process.ExitCode == 0;
                    ActivityRunResult = runRez == true ? 1 : -1;
                    _stopwatch.Stop();
                    _timer.Stop();

                });
                rezThread.Start();
                return new ActivityStartInfo(rezThread, null, CommonOperationResult.SayOk());
            }
            catch(Exception ex)
            {
                errText = $"ERROR message={ex.Message}, innerexception={ex.InnerException}";
                logger.Error(errText);
                return new ActivityStartInfo(null, null, CommonOperationResult.SayFail(errText));
            }
             
        }
    }
}
