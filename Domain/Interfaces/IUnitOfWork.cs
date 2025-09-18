using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        IUserRepository UserRepository { get; }
        Task<int> CompleteAsync();
    }
}
