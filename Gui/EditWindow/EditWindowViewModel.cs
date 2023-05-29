using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Shared.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Gui.EditWindow
{
    internal class EditWindowViewModel
    {
        private Serilog.ILogger _logger;
        private IAsyncRepositoryT<Activity> _repo;
        private BatchManager _batchManager;
        private ActivityManager _activityManager;

        private Batch CurrentBatch { get; set; }
        private Activity CurrentActivity { get; set; }
        private List<Activity> _activitiesList = new List<Activity>();
        private TimeSpan _actStartTime { get; set; }
        private bool _saveActivityStopMarker = false;
        public string BufferIn { get; set; }

        EditWindowViewModel(BatchManager batchManager, ActivityManager activityManager, Batch currentBatch, Serilog.ILogger logger)
        {
            _logger = logger;
            _batchManager = batchManager;
            CurrentBatch = _batchManager.Clone(currentBatch);
            _activityManager = activityManager;

            _batchManager._checker.BindControlToCheck("UpdateNumber", BatchNumberTb).BindControlToCheck("UpdateName", BatchNameTb);

            _activityManager._checker.BindActionToCheck("UpdateTransactionId", () => { TransactionIdTb.Focus(); TransactionIdTb.SelectAll(); });

        }



    }


}
