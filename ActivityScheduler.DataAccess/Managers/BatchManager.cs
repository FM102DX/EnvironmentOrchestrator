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
    public class BatchManager
    {
        //class that incapsulates all business logic related to Batch entity

        private IAsyncRepositoryT<Batch> _repo;

        private ActivityManager _activityManager;

        public BatchManager(IAsyncRepositoryT<Batch> repo, ActivityManager activityManager)
        {
            _repo = repo;
            _activityManager = activityManager;
        }

        public Task<CommonOperationResult> CheckNumber(string number)
        {
            return Task.FromResult(Validation.CheckIf6DigitTrasactionNumberIsCorrect(number));
        }
        public Task<CommonOperationResult> CheckName(string name)
        {
            return Task.FromResult(Validation.CheckIfTransactionOrBatchNameIsCorrect(name));
        }

        public Task<List<Batch>> GetAll()
        {
            return Task.FromResult(_repo.GetAllAsync().Result.ToList());
        }

        public Task<CommonOperationResult> AddNewBatch(Batch batch)
        {
            //check if number and name is unique

            if (batch.Number!="000000") 
            {
                var batches = _repo.GetAllAsync().Result.ToList();

                var numberCount = batches.Where(x => x.Number == batch.Number).ToList().Count;
                if (numberCount > 0)
                {
                    return Task.FromResult(CommonOperationResult.SayFail($"The number you entered already exists"));
                }

                var nameCount = batches.Where(x => x.Name == batch.Name).ToList().Count;
                if (nameCount > 0)
                {
                    return Task.FromResult(CommonOperationResult.SayFail($"The name you entered already exists"));
                }
            }

            var rez=_repo.AddAsync(batch);

            return rez;
        }

        public Task<CommonOperationResult> RemoveBatch(Guid id)
        {
            try
            {
                return _repo.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                return Task.FromResult(CommonOperationResult.SayFail($"Failed to remove batch, exception={ex.Message}, innerexception={ex.InnerException}"));
            }
        }

        public Task<CommonOperationResult> RemoveAllBatches()
        {
            try
            {
                var items = _repo.GetAllAsync().Result.ToList();

                items.ForEach(x => _activityManager.RemoveAllBatchActivities(x.Id));
                
                return Task.FromResult(CommonOperationResult.SayOk());
            }
            catch (Exception ex)
            {
                return Task.FromResult(CommonOperationResult.SayFail($"Failed to remove batches, exception={ex.Message}, innerexception={ex.InnerException}"));
            }
        }
    }
}
