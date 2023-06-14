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
using ActivityScheduler.Data.Executors;

namespace ActivityScheduler.Data.Managers
{
    public class BatchManager
    {
        //class that incapsulates all business logic related to Batch entity

        private IAsyncRepositoryT<Batch> _repo;

        private ActivityManager _activityManager;

        //TODO make this more accurate
        public CheckExecutor<Batch> _checker = new CheckExecutor<Batch>();

        private List<Batch> _batches= new List<Batch>();

        public BatchManager(IAsyncRepositoryT<Batch> repo, ActivityManager activityManager)
        {
            _repo = repo;
            _activityManager = activityManager;
            _checker
                    .AddCheck(new List<string>() { "Update" }, "Number", (Batch batch) =>
                    {
                        if (batch.Number == "000000") {return CommonOperationResult.SayFail($"Please enter number different from 000000"); }

                        var rez = Validation.CheckIf6DigitTrasactionNumberIsCorrect(batch.Number);
                        if (!rez.Success) { return rez; }

                        _batches = _repo.GetAllAsync().Result.ToList();
                        if (_batches.Count == 0) { return CommonOperationResult.SayOk(); }

                        var numberCount = _batches.Where(x => x.Number == batch.Number && x.Id != batch.Id).ToList().Count;
                        if (numberCount > 0)
                        {
                            return CommonOperationResult.SayFail($"The number you entered already exists");
                        }
                        return CommonOperationResult.SayOk();
                    })
                    .AddCheck(new List<string>() { "Insert", "Update" }, "Name", (Batch batch) =>
                    {
                        if (batch.Name.ToLower() == "new.group" && batch.IsGroup) 
                        { 
                            return CommonOperationResult.SayOk();
                        }
                        
                        if (batch.Name.ToLower() == "new.batch" && (!batch.IsGroup))
                        {
                            return CommonOperationResult.SayOk();
                        }

                        var rez = Validation.CheckIfTransactionOrBatchNameIsCorrect(batch.Name);
                        
                        if (!rez.Success) { return rez; }

                        var nameCount = _batches.Where(x => x.Name == batch.Name && x.Id != batch.Id).ToList().Count;

                        if (nameCount > 0)
                        {
                            return CommonOperationResult.SayFail($"The name you entered already exists");
                        }

                        return CommonOperationResult.SayOk();
                    })
                    .AddCheck(new List<string>() { "Insert" }, "Number", (Batch batch) =>
                    {
                        if (batch.Number == "000000") 
                        {
                            return CommonOperationResult.SayOk(); 
                        }

                        var rez = Validation.CheckIf6DigitTrasactionNumberIsCorrect(batch.Number);
                        if (!rez.Success) { return rez; }

                        _batches = _repo.GetAllAsync().Result.ToList();
                        if (_batches.Count == 0) { return CommonOperationResult.SayOk(); }

                        var numberCount = _batches.Where(x => x.Number == batch.Number && x.Id != batch.Id).ToList().Count;
                        if (numberCount > 0)
                        {
                            return CommonOperationResult.SayFail($"The number you entered already exists");
                        }
                        return CommonOperationResult.SayOk();
                    });
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
            var btc = _checker.PerformCheck("Insert", batch);

            if (!btc.Success)
            {
                return Task.FromResult(btc);
            }

            var rez = _repo.AddAsync(batch);

            return rez;
        }

        public Task<CommonOperationResult> ModifyBatch(Batch batch)
        {
            var btc = _checker.PerformCheck("Update", batch);

            if (!btc.Success)
            {
                return Task.FromResult(btc);
            }
            var rez = _repo.UpdateAsync(batch);

            return rez;
        }

        public Task<CommonOperationResult> RemoveBatch(Guid id)
        {
            try
            {

                var remRez= _activityManager.RemoveAllBatchActivities(id);

                if (!remRez.Result.Success)
                {
                    return Task.FromResult(CommonOperationResult.SayFail($"Cannot remove batch because cant remove its activities"));
                }

                //cant remove batch if it contains activities
                var actv = _activityManager.GetAll(id).Result;

                if (actv.Count>0)
                {
                    return Task.FromResult(CommonOperationResult.SayFail($"Cannot remove batch because it contains {actv.Count} activities"));
                }

                var delRez=_repo.DeleteAsync(id).Result;

                if (!delRez.Success)
                {
                    return Task.FromResult(CommonOperationResult.SayFail($"Cannot remove batch because of error: {delRez.Message}"));
                }
                else
                {
                    return _activityManager.RemoveAllBatchActivities(id);
                }

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

                foreach (var x in items)
                {
                    var rez = RemoveBatch(x.Id).Result;

                    if (!rez.Success)
                    {
                        return Task.FromResult(CommonOperationResult.SayFail($"Failed to remove batch={rez.Message}"));
                    }
                }
                return Task.FromResult(CommonOperationResult.SayOk());
            }
            catch (Exception ex)
            {
                return Task.FromResult(CommonOperationResult.SayFail($"Failed to remove batches, exception={ex.Message}, innerexception={ex.InnerException}"));
            }
        }

        public Batch Clone(Batch sourceBatch)
        {
            Batch newBatch = new Batch();
            newBatch.Id = sourceBatch.Id;
            newBatch.Name = sourceBatch.Name;
            newBatch.Interval = sourceBatch.Interval;
            newBatch.Duration = sourceBatch.Duration;
            newBatch.RunMode = sourceBatch.RunMode;
            newBatch.Number = sourceBatch.Number;
            newBatch.IsGroup = sourceBatch.IsGroup;
            newBatch.DefaultScriptPath = sourceBatch.DefaultScriptPath;
            newBatch.ActiveDaysOfWeek = sourceBatch.ActiveDaysOfWeek;
            newBatch.DefaultScriptPath = sourceBatch.DefaultScriptPath;
            return newBatch;
        }
    }
}
