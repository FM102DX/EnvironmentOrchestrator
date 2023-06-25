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
using ActivityScheduler.Data.Executors;

namespace ActivityScheduler.Data.Managers
{
    public class ActivityManager
    {
        private IAsyncRepositoryT<Activity> _repo;
        public CheckExecutor<Activity> _checker = new CheckExecutor<Activity>();
        public List<Activity> _activitiesList = new List<Activity>();
        private Batch _currentBatch;

        public ActivityManager(IAsyncRepositoryT<Activity> repo)
        {
            _repo = repo;
            _checker
            .AddCheck(new List<string>() { "Update" }, "ActivityId", (Activity activity) => {

                //get all activities of this batch
                _activitiesList = _repo.GetAllAsync(x => x.BatchId == activity.BatchId).Result.ToList();

                var rez = Validation.CheckActivityId(activity.ActivityId.ToString());
                if (!rez.Success) { return rez; }

                //ActivityId should be unique
                var uniqueActivities = _activitiesList.Where(x=>(x.Id!= activity.Id) &&(x.ActivityId== activity.ActivityId)).ToList();
                if(uniqueActivities.Count>0)
                {
                    return CommonOperationResult.SayFail($"Activity with Id = {activity.ActivityId} already exists in this batch");
                }

                return CommonOperationResult.SayOk();


            })
            .AddCheck(new List<string>() { "Insert" }, "ActivityId", (Activity activity) => {

                if (activity.ActivityId.ToString() == "000") { return CommonOperationResult.SayOk(); }

                //get all activities of this batch
                _activitiesList = _repo.GetAllAsync(x => x.BatchId == activity.BatchId).Result.ToList();

                var rez = Validation.CheckActivityId(activity.ActivityId.ToString());
                if (!rez.Success) { return rez; }

                //ActivityId should be unique
                var uniqueActivities = _activitiesList.Where(x => (x.Id != activity.Id) && (x.ActivityId != activity.ActivityId)).ToList();
                if (uniqueActivities.Count > 0)
                {
                    return CommonOperationResult.SayFail($"Activity with Id = {activity.ActivityId} already exists in this batch");
                }

                return CommonOperationResult.SayOk();


            })
            .AddCheck(new List<string>() { "Update", "Insert" }, "ActivityId", (Activity activity) => {

                            if (activity.ActivityId > 9999 || activity.ActivityId<1)
                {
                                return CommonOperationResult.SayFail($"ActivityId is a number between 1 and 9999");
                            }

                            return CommonOperationResult.SayOk();
            })
            .AddCheck(new List<string>() { "Update", "Insert" }, "Name", (Activity activity) => {

            var rez = Validation.CheckIfTransactionOrBatchNameIsCorrect(activity.Name);
                if (!rez.Success) { return rez; }

                //ActivityName should be unique
                var uniqueActivities = _activitiesList.Where(x => (x.Id != activity.Id) && (x.Name == activity.Name)).ToList();
                if (uniqueActivities.Count > 0)
                {
                    return CommonOperationResult.SayFail($"Activity with Name = {activity.Name} already exists in this batch");
                }

                return CommonOperationResult.SayOk();
            })
            .AddCheck(new List<string>() { "Update", "Insert" }, "TransactionId", (Activity activity) => {
                var rez = Validation.CheckIf6DigitTrasactionNumberIsCorrect(activity.TransactionId);
                if (!rez.Success) { return rez; }
                return CommonOperationResult.SayOk();
            })
            .AddCheck(new List<string>() { "Update", "Insert" }, "Starttime", (Activity activity) => {
                if (activity.StartTime.TotalSeconds < 0) { return CommonOperationResult.SayFail("Activity start timepoint cant be negative"); }
                return CommonOperationResult.SayOk();
             });
        }

        public ActivityManager SetCurrentBatch(Batch currentBatch)
        {
            _currentBatch = currentBatch;
            return this;
        }
        public Task<List<Activity>> GetAll()
        {
            var rezList= _repo.GetAllAsync().Result.ToList();
            rezList.ForEach(x => { x.ParentBatch = _currentBatch; });
            return Task.FromResult(rezList);
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
        public Task<CommonOperationResult> ModifyActivity(Activity activity)
        {
            var btc = _checker.PerformCheck("Update", activity);

            if (!btc.Success)
            {
                return Task.FromResult(btc);
            }

            var rez = _repo.UpdateAsync(activity);

            return rez;
        }

        public Task<CommonOperationResult> RemoveActivity(Guid id)
        {
            var delRez = _repo.DeleteAsync(id);

            if (!delRez.Result.Success)
            {
                return Task.FromResult(CommonOperationResult.SayFail($"Cannot remove activity because of error: {delRez.Result.Message}"));
            }



            return delRez;
        }

        public bool Similar(Activity? activityOriginal, Activity? activityCompare)
        {
            if (activityOriginal == null && activityCompare == null)
            {
                return true;
            }

            if (activityOriginal == null || activityCompare==null) 
            { 
                return false; 
            }

            bool rez =  activityOriginal.Id == activityCompare.Id &&
                        activityOriginal.BatchId == activityCompare.BatchId &&
                        activityOriginal.Name == activityCompare.Name &&
                        activityOriginal.ActivityId == activityCompare.ActivityId &&
                        activityOriginal.AlwaysSuccess == activityCompare.AlwaysSuccess &&
                        activityOriginal.StartTime == activityCompare.StartTime &&
                        activityOriginal.TransactionId == activityCompare.TransactionId &&
                        activityOriginal.IsDomestic == activityCompare.IsDomestic &&
                        activityOriginal.IsActive == activityCompare.IsActive &&
                        activityOriginal.IsHub == activityCompare.IsHub &&
                        activityOriginal.ChildDelay == activityCompare.ChildDelay &&
                        activityOriginal.ParentActivities == activityCompare.ParentActivities &&
                        activityOriginal.ActivityParentRule == activityCompare.ActivityParentRule &&
                        activityOriginal.ScriptPath == activityCompare.ScriptPath;

            return rez;

        }
        public Activity Clone(Activity source)
        {
            Activity acv = new Activity();
            acv.Id = source.Id;
            acv.BatchId = source.BatchId;
            acv.Name = source.Name;
            acv.ActivityId = source.ActivityId;
            acv.AlwaysSuccess = source.AlwaysSuccess;
            acv.StartTime = source.StartTime;
            acv.IsActive = source.IsActive;
            acv.TransactionId = source.TransactionId;
            acv.IsDomestic= source.IsDomestic;
            acv.IsHub= source.IsHub;
            acv.ChildDelay = source.ChildDelay;
            acv.ParentActivities = source.ParentActivities;
            acv.ActivityParentRule= source.ActivityParentRule;
            acv.ScriptPath = source.ScriptPath;
            return acv;
        }
    }
}
