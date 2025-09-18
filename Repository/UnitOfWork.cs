using Domain.Entities;
using Domain.Interfaces;
using Repository.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbcontext;
        private Hashtable _repositories;
        private IUserRepository _userRepository;
        private bool _disposed = false;

        public UnitOfWork(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
            _repositories = new Hashtable();
        }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var Key = typeof(T).Name;
            if (!_repositories.ContainsKey(Key))
            {
                var repository = new GenericRepository<T>(_dbcontext);
                _repositories.Add(Key, repository);
            }
            return _repositories[Key] as IGenericRepository<T>;
        }

        public IUserRepository UserRepository
        {
            get
            {
                _userRepository ??= new UserRepository(_dbcontext);
                return _userRepository;
            }
        }

        // Commit changes asynchronously
        public async Task<int> CompleteAsync()
        {
            return await _dbcontext.SaveChangesAsync();
        }

        // Synchronous disposal
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Asynchronous disposal
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _dbcontext?.Dispose();
                _disposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_dbcontext != null && !_disposed)
            {
                await _dbcontext.DisposeAsync();
                _disposed = true;
            }
        }
    }
}
