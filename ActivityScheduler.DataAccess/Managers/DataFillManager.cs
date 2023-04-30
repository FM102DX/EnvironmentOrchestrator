using ActivityScheduler.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Managers
{
    public class DataFillManager
    {
        private BatchManager _batchManager;
        private ActivityManager _activityManager;
        public DataFillManager(BatchManager batchManager, ActivityManager activityManager) 
        {
            _batchManager=batchManager;
            _activityManager=activityManager;
        }

        public void FillTheModel()
        {
            _batchManager.RemoveAllBatches();

            Batch batch1 = new Batch();
            batch1.Number = "100101";
            batch1.Name = "Env.checkin";
            _batchManager.AddNewBatch(batch1);

            Activity actv1 = new Activity()
            {
                ActivityId = 100,
                BatchId = batch1.Id,
                Name = "Check01",
                TransactionId = "350101"
            };

            Activity actv2 = new Activity()
            {
                ActivityId = 200,
                BatchId = batch1.Id,
                Name = "Check02",
                TransactionId = "350102"
            };

            _activityManager.AddNewActivity(actv1);
            _activityManager.AddNewActivity(actv2);

            Batch batch2 = new Batch();
            batch2.Number = "200101";
            batch2.Name = "15.min.standard.test";
            _batchManager.AddNewBatch(batch2);


            Activity actv3 = new Activity()
            {
                ActivityId = 100,
                BatchId = batch2.Id,
                Name = "Configure.test",
                TransactionId = "780101"
            };

            Activity actv4 = new Activity()
            {
                ActivityId = 200,
                Name = "Start.test",
                TransactionId = "780102"
            };

            _activityManager.AddNewActivity(actv3);
            _activityManager.AddNewActivity(actv4);

        }

    }
}
