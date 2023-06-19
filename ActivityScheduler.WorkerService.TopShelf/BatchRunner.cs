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
    public  class BatchRunner
    {
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private ActivityManager _activityManager;

        private List<string> _runningBatches = new List<string>();

        public BatchRunner(BatchManager batchManager, ActivityManager activityManager, Serilog.ILogger logger)
        {
            _batchManager=batchManager;
            _activityManager=activityManager;
            _logger =logger;
        }
        public CommonOperationResult RunBatch(string batchId) 
        {
            if (_runningBatches.Contains(batchId))
            {
                return CommonOperationResult.SayFail($"Cant start batch {batchId} because its already running");
            }
            _runningBatches.Add(batchId);
            return CommonOperationResult.SayOk();
        }

        public CommonOperationResult StopBatch(string batchId)
        {
            if (!_runningBatches.Contains(batchId))
            {
                return CommonOperationResult.SayFail($"Cant stop batch {batchId} because its not running");
            }
            
            _runningBatches.RemoveAll(x => x == batchId);

            return CommonOperationResult.SayOk();
        }
        public RunningBatchesInfo GetRunningBatchesInfo()
        {
            return new RunningBatchesInfo()
            {
                Batches = _runningBatches
            };
        }

        public string GetRunBatchList()
        {
            return string.Join(",", _runningBatches);
        }

        public int GetRunBatchCount()
        {
            return _runningBatches.Count;
        }

        public CommonOperationResult RunBatch0(string batchId)
        {

        }

        public void RunBatchOnce(DateTime startDateTime, Batch batch)
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
