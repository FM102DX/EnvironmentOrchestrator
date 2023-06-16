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

        private List<string> _batches = new List<string>();

        public BatchRunner(BatchManager batchManager, ActivityManager activityManager, Serilog.ILogger logger)
        {
            _batchManager=batchManager;
            _activityManager=activityManager;
            _logger =logger;
        }
        public CommonOperationResult RunBatch(string batchId) 
        {
            if (_batches.Contains(batchId))
            {
                return CommonOperationResult.SayFail($"Cant start batch {batchId} because its already running");
            }
            _batches.Add(batchId);
            return CommonOperationResult.SayOk();
        }

        public CommonOperationResult StopBatch(string batchId)
        {
            if (!_batches.Contains(batchId))
            {
                return CommonOperationResult.SayFail($"Cant stop batch {batchId} because its not running");
            }
            
            _batches.RemoveAll(x => x == batchId);

            return CommonOperationResult.SayOk();
        }
        public RunningBatchesInfo GetRunningBatchesInfo()
        {
            return new RunningBatchesInfo()
            {
                Batches = _batches
            };
        }

        public string GetRunBatchList()
        {
            return string.Join(",", _batches);
        }

        public int GetRunBatchCount()
        {
            return _batches.Count;
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


                        if (isRunningTimeNow  && (activity.Status == ActivityStatusEnum.Waiting))
                        {
                            //launch task
                            //run powershell with params



                        }



                    }
                }
                Thread.Sleep(500);
            }
            while (!canExit);
        }
    }
}
