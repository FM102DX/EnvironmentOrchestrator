using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ActivityScheduler.Data.DataAccess;
using ActivityScheduler.Shared;

namespace ActivityScheduler.Data.Contracts
{
    public interface IAsyncRepositoryT<T> where T : BaseEntity
    {
        public Task<IEnumerable<T>> GetAllAsync();

        public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter);

        public Task<List<T>> GetItemsListAsync();

        public Task<T> GetByIdOrNullAsync(Guid id);

        public Task<int> Count { get; }

        public  Task<bool> Exists(Guid id);

        public Task<CommonOperationResult> AddAsync(T t);

        public Task<CommonOperationResult> UpdateAsync(T t);

        public Task<CommonOperationResult> DeleteAsync(Guid id);

        public Task<CommonOperationResult> InitAsync(bool deleteDb = false);

        public Task<T> GetRandomObjectAsync();

        public Task<List<T>> GetRandomListAsync(int count);


    }
}
