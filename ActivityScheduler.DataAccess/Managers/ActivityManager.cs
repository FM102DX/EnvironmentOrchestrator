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

namespace ActivityScheduler.Data.Managers
{
    public class ActivityManager
    {
        private IAsyncRepositoryT<Activity> _repo;

        public ActivityManager(IAsyncRepositoryT<Activity> repo)
        {
            _repo = repo;
        }

        public Task<CommonOperationResult> CheckNumber(string number)
        {
            return Task.FromResult(Validation.CheckIf6DigitTrasactionNumberIsCorrect(number));
        }
        public Task<CommonOperationResult> CheckName(string name)
        {
            return Task.FromResult(Validation.CheckIfTransactionOrBatchNameIsCorrect(name));
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
    }
}
