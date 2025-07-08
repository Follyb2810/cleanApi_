using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cleanApi.Common.Interfaces
{
     public interface IRepository
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
    }
}