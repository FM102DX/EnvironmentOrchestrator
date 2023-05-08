using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models.Communication;
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
        BatchManager _batchManager;
        Serilog.ILogger _logger;

        List<string> _batches = new List<string>();

        public BatchRunner(BatchManager batchManager, Serilog.ILogger logger)
        {
            _batchManager=batchManager;
            _logger=logger;
        }
        public void RunBatch(string BatchId) 
        {
            _batches.Add(BatchId);  
        }

        public void StopBatch(string BatchId)
        {
            _batches.RemoveAll(x => x == BatchId);
        }
        public RunningBatchesInfo GetRunningBatchesInfo()
        {
            return new RunningBatchesInfo()
            {
                Batches = _batches
            };
        }

    }
}
