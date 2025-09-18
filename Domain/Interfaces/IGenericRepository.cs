using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync<TKey>(TKey id);
        Task<T> AddAsync<TEntity>(TEntity entity) where TEntity : T;
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
