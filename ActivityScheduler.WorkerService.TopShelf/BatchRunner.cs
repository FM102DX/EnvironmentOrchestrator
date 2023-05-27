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

        private List<string> _batches = new List<string>();

        public BatchRunner(BatchManager batchManager, Serilog.ILogger logger)
        {
            _batchManager=batchManager;
            _logger=logger;
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

    }
}
