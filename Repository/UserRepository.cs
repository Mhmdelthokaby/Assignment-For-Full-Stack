using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<User>();
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _dbSet.AddAsync(user);
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _dbSet.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _dbSet.Remove(user);
            await Task.CompletedTask;
        }

        public async Task<User> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}
