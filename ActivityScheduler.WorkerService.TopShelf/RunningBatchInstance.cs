using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public  class RunningBatchInstance
    {
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private ActivityManager _activityManager;
        string _batchNumber;
        bool _stopMarker;
        private readonly System.Timers.Timer _timer;

        public RunningBatchInstance(string batchNumber, BatchManager batchManager, ActivityManager activityManager, Serilog.ILogger logger)
        {
            _batchManager=batchManager;
            _activityManager=activityManager;
            _logger =logger;
            _batchNumber = batchNumber;
            
            //timer
            //_timer = new System.Timers.Timer(500) { AutoReset = true };
            //_timer.Elapsed += _timer_Elapsed;
            //_timer.Start();
        }

        public CommonOperationResult Run() 
        {
            _stopMarker = false;
            
            do
            {
                Thread.Sleep(500);
            }
            while (!_stopMarker);
            _logger.Information($"010024 RunningBatchInstance: Exiting run cycle");
            return CommonOperationResult.SayOk();
        }

        public CommonOperationResult Stop()
        {
            _stopMarker=true;
            _logger.Information($"010024 RunningBatchInstance: Stop command");
            return CommonOperationResult.SayOk();
        }

        private CommonOperationResult RunBatch0()
        {
            return CommonOperationResult.SayOk();
        }

        private void RunBatchOnce(DateTime startDateTime, Batch batch)
        {
            var activities = _activityManager.GetAll(batch.Id).Result.ToList().OrderBy(x=>x.ActivityId).ToList();
            
            bool canExit=false;

            do 
            {
                foreach (var activity in activities)
                {
                    if (activity.IsActive)
                    {
                        if (activity.Status == ActivityStatusEnum.Idle)
                        {
                            activity.Status = ActivityStatusEnum.Waiting;
                        }

                        bool isRunningTimeNow = DateTime.Now > startDateTime + activity.StartTime;

                        //get script path
                        string? scriptPath = batch.DefaultScriptPath;
                        if (!string.IsNullOrEmpty(activity.ScriptPath)) { scriptPath = activity.ScriptPath; }
                        if(string.IsNullOrEmpty(scriptPath))
                        {
                            throw new Exception($"No ScriptPath value specified for batch or activity, batch number={batch.Number}");
                        }

                        if (isRunningTimeNow  && (activity.Status == ActivityStatusEnum.Waiting))
                        {
                            //launch task
                            //run powershell with params
                            
                            string appName = "powershell.exe";
                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                            startInfo.CreateNoWindow = false;
                            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                            startInfo.FileName = appName;
                            startInfo.Arguments = $"-file {scriptPath} -transactionId {activity.TransactionId}";
                            process.StartInfo = startInfo;
                            process.Start();
                        }

                    }
                }
                Thread.Sleep(500);
            }
            while (!canExit);
        }
    }
}
