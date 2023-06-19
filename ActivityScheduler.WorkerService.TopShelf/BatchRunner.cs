using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Pipes;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public class BatchRunner
    {
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private ActivityManager _activityManager;
        private readonly System.Timers.Timer _timer;
        private List<BatchRunningInfo> _runningBatches = new List<BatchRunningInfo>();

        public event TaskCompletedDelegate  TaskCompleted;
        public delegate void TaskCompletedDelegate(TaskCompletedInfo taskCompletedInfo);


        public BatchRunner(BatchManager batchManager, ActivityManager activityManager, Serilog.ILogger logger)
        {
            _batchManager = batchManager;
            _activityManager = activityManager;
            _logger = logger;
            _timer = new System.Timers.Timer(500) { AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //go through _runningBatches looking for completed tasks, raise event, remove task 

        }

        private bool IsBatchRunning(string batchNumber)
        {
            return _runningBatches.Where(x => x.BatchNumber== batchNumber).ToList().Count() > 0;   
        }

        public CommonOperationResult RunBatch(string batchNumber)
        {
            if (IsBatchRunning(batchNumber))
            {
                return CommonOperationResult.SayFail($"Cant start batch {batchNumber} because its already running");
            }

            try
            {
                var rez = Task.Run(() => {
                    var instance = new RunningBatchInstance(batchNumber, _batchManager, _activityManager, _logger);
                    instance.Run();
                });
                _runningBatches.Add(new BatchRunningInfo() { BatchNumber= batchNumber, BatchRunTask= rez });
            }
            catch (Exception ex)
            {
                return CommonOperationResult.SayFail($"Filed to start batch {batchNumber} exception is: {ex.Message}, innerexception is {ex.InnerException}");
            }

            return CommonOperationResult.SayOk();
        }

        public CommonOperationResult StopBatch(string batchNumber)
        {
            if (!IsBatchRunning(batchNumber))
            {
                return CommonOperationResult.SayFail($"Cant stop batch {batchNumber} because its not running");
            }

            ////

            return CommonOperationResult.SayOk();
        }
        public RunningBatchesInfo GetRunningBatchesInfo()
        {
            return new RunningBatchesInfo()
            {
                Batches = _runningBatches.Select(x =>x.BatchNumber).ToList()
            };
        }

        public int GetRunBatchCount()
        {
            return _runningBatches.Count;
        }

        public class BatchRunningInfo
        {
            public string? BatchNumber { get; set; }
            public Task? BatchRunTask { get; set; }
        }
        public class TaskCompletedInfo
        {
            public string? BatchNumber { get; set; }
            public CommonOperationResult? Result { get; set; }
        }

    }

}
