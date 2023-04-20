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

namespace ActivityScheduler.Data.Managers
{
    public class BatchManager
    {
        //class that incapsulates all business logic related to Batch entity

        private IAsyncRepositoryT<Batch> _repo;

        public BatchManager(IAsyncRepositoryT<Batch> repo)
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

        public Task<List<Batch>> GetAll()
        {
            return Task.FromResult(_repo.GetAllAsync().Result.ToList());
        }

        public Task<CommonOperationResult> AddNewBatch(Batch batch)
        {
            //check if number and name is unique

            var batches= _repo.GetAllAsync().Result.ToList();

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

            var rez=_repo.AddAsync(batch);

            return rez;
        }
    }
}
