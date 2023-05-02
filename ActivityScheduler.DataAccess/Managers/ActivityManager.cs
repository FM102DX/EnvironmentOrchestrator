﻿using ActivityScheduler.Data.Contracts;
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
        //private Batch _currentBatch;

        public ActivityManager(IAsyncRepositoryT<Activity> repo)
        {
            _repo = repo;
            _checker
            .AddCheck(new List<string>() { "update" }, "ActivityId", (Activity activity) => {

                //get all activities of this batch
                _activitiesList = _repo.GetAllAsync(x => x.BatchId == activity.BatchId).Result.ToList();

                var rez = Validation.CheckActivityId(activity.ActivityId.ToString());
                if (!rez.Success) { return rez; }

                //ActivityId should be unique
                var uniqueActivities = _activitiesList.Where(x=>(x.Id!= activity.Id) &&(x.ActivityId!= activity.ActivityId)).ToList();
                if(uniqueActivities.Count>0)
                {
                    return CommonOperationResult.SayFail($"Activity with Id = {activity.ActivityId} already exists in this batch");
                }

                return CommonOperationResult.SayOk();


            })
            .AddCheck(new List<string>() { "insert" }, "ActivityId", (Activity activity) => {

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
            .AddCheck(new List<string>() { "update", "insert" }, "Name", (Activity activity) => {

                var rez = Validation.CheckIfTransactionOrBatchNameIsCorrect(activity.Name);
                if (!rez.Success) { return rez; }

                //ActivityName should be unique
                var uniqueActivities = _activitiesList.Where(x => (x.Id != activity.Id) && (x.Name != activity.Name)).ToList();
                if (uniqueActivities.Count > 0)
                {
                    return CommonOperationResult.SayFail($"Activity with Name = {activity.Name} already exists in this batch");
                }

                return CommonOperationResult.SayOk();
            })
            .AddCheck(new List<string>() { "update", "insert" }, "TransactionId", (Activity activity) => {

                var rez = Validation.CheckIf6DigitTrasactionNumberIsCorrect(activity.TransactionId);
                if (!rez.Success) { return rez; }

                return CommonOperationResult.SayOk();
            })
            .AddCheck(new List<string>() { "update", "insert" }, "Starttime", (Activity activity) => {
                if (activity.StartTime.TotalSeconds < 0) { return CommonOperationResult.SayFail("Activity start timepoint cant be negative"); }
                return CommonOperationResult.SayOk();
             });
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
