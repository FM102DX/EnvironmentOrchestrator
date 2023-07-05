using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Data.Models.Settings;
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
        private System.Timers.Timer _timer;
        private List<BatchRunningInfo> _runningBatches = new List<BatchRunningInfo>();
        public event TaskCompletedDelegate  TaskCompleted;
        public delegate void TaskCompletedDelegate(TaskCompletedInfo taskCompletedInfo);
        private SettingsManager _settingsManager;

        public BatchRunner(BatchManager batchManager, ActivityManager activityManager, SettingsManager settingsManager, Serilog.ILogger logger)
        {
            _batchManager = batchManager;
            _activityManager = activityManager;
            _logger = logger;
            _timer = new System.Timers.Timer(500) { AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            _settingsManager = settingsManager;
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //go through _runningBatches looking for completed tasks, raise event, remove task 
            //_logger.Information($"BatchRunner: 1200070 clearing tasklist _runningBatches.Count={_runningBatches.Count}");
            foreach (BatchRunningInfo batchRunningInfo in _runningBatches)
            {
                if (batchRunningInfo.BatchRunTask.IsCompleted)
                {
                    TaskCompleted(new TaskCompletedInfo() 
                    { 
                        BatchNumber = batchRunningInfo.BatchNumber, 
                        Result = batchRunningInfo.BatchRunTask.Result 
                    });
                }

            }
            _runningBatches.ForEach(x => _logger.Information($"_runningBatches dump {x.BatchNumber}--{x.BatchRunTask.Status}"));
            

            _runningBatches.RemoveAll(x => x.BatchRunTask.IsCompleted);
           //  _logger.Information($"BatchRunner: 1200070 DONE clearing tasklist _runningBatches.Count={_runningBatches.Count}");
        }

        private bool IsBatchRunning(string batchNumber)
        {
            return _runningBatches.Where(x => x.BatchNumber== batchNumber).ToList().Count() > 0;   
        }

        public CommonOperationResult RunBatch(string batchNumber)
        {
            _logger.Information($"BatchRunner: Attempt to run batch {batchNumber}");
            if (IsBatchRunning(batchNumber))
            {
                return CommonOperationResult.SayFail($"Cant start batch {batchNumber} because its already running");
            }
            try
            {
                var instance = new RunningBatchInstance(batchNumber, _batchManager, _activityManager, _settingsManager, _logger);
                var rez = Task<CommonOperationResult>.Run(() => {
                    return instance.Run();
                });
                _runningBatches.Add(new BatchRunningInfo() { BatchNumber= batchNumber, BatchRunTask= rez, Instance= instance });
                //_logger.Information($"010024 Adding batch {batchNumber} to running ones, now {_runningBatches.Count} tasks in list, {_runningBatches.Where(x=>x.BatchRunTask.IsCompleted).ToList().Count} completed");
            }
            catch (Exception ex)
            {
                return CommonOperationResult.SayFail($"010024 Filed to start batch {batchNumber} exception is: {ex.Message}, innerexception is {ex.InnerException}");
            }
            return CommonOperationResult.SayOk();
        }

        public CommonOperationResult StopBatch(string batchNumber)
        {
            if (!IsBatchRunning(batchNumber))
            {
                return CommonOperationResult.SayFail($"Cant stop batch {batchNumber} because its not running");
            }

            var x=_runningBatches.Where(x => x.BatchNumber== batchNumber).ToList().FirstOrDefault();

            if(x!=null)
            {
                var stopRez = x.Instance.Stop();

                if (stopRez.Success)
                {
                    _logger.Information($"010024 Remmoving batch {batchNumber} from running ones");
                }
            }

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
            public Task<CommonOperationResult>? BatchRunTask { get; set; }

            public RunningBatchInstance Instance { get; set; }
        }
        public class TaskCompletedInfo
        {
            public string? BatchNumber { get; set; }
            public CommonOperationResult? Result { get; set; }
        }

    }

}
