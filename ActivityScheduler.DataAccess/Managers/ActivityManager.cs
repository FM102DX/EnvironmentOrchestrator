using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using ActivityScheduler.Shared.Validation;
using Activity = ActivityScheduler.Data.Models.Activity;
using System.Xml.Linq;

namespace ActivityScheduler.Data.Managers
{
    public class ActivityManager
    {
        private IAsyncRepositoryT<Activity> _repo;

        public ActivityManager(IAsyncRepositoryT<Activity> repo)
        {
            _repo = repo;
        }

        public Task<List<Activity>> GetAll()
        {
            return Task.FromResult(_repo.GetAllAsync().Result.ToList());
        }

        public Task<CommonOperationResult> AddNewActivity(Activity activity)
        {
            var rez = _repo.AddAsync(activity);
            return rez;
        }

        public Task<CommonOperationResult> RemoveAllBatchActivities(Guid batchId)
        {
            try
            {
                var items = _repo.GetAllAsync(x => x.BatchId == batchId).Result.ToList();
                items.ForEach(x => _repo.DeleteAsync(x.Id));
                return Task.FromResult(CommonOperationResult.SayOk());
            }
            catch (Exception ex)
            {
                return Task.FromResult(CommonOperationResult.SayFail($"Failed to remove activities, exception={ex.Message}, innerexception={ex.InnerException}"));
            }
        }
        public Task<List<Activity>> GetAll(Guid batchId)
        {
            return Task.FromResult(_repo.GetAllAsync(x => x.BatchId == batchId).Result.ToList());
        }

        public Activity Clone(Activity sca)
        {
            Activity acv = new Activity();
            acv.Id = sca.Id;
            acv.Name = sca.Name;
            acv.ActivityId = sca.ActivityId;
            acv.AlwaysSuccess = sca.AlwaysSuccess;
            acv.StartTime = sca.StartTime;
            acv.TransactionId = sca.TransactionId;
            acv.IsDomestic = sca.IsDomestic;
            acv.IsHub = sca.IsHub;
            acv.ChildDelay = sca.ChildDelay;
            sca.ParentIds.ForEach(x => acv.ParentIds.Add(x));
            return acv;
        }
    }
}
