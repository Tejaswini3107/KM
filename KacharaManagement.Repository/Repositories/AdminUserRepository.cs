using KacharaManagement.Core.Entities;
using KacharaManagement.Repository.Data;
using KacharaManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KacharaManagement.Repository.Repositories
{
    public class AdminUserRepository : IAdminUserRepository
    {
        private readonly GothamDbContext _context;
        public AdminUserRepository(GothamDbContext context)
        {
            _context = context;
        }

        public async Task<AdminUser?> GetByUsernameAndPasswordAsync(string username, string password)
        {
            return await _context.AdminUsers.FirstOrDefaultAsync(x => x.Username == username && x.PasswordHash == password);
        }

        public async Task<List<AdminUser>> GetAllAsync()
        {
            return await _context.AdminUsers.ToListAsync();
        }

        public async Task AddAsync(AdminUser user)
        {
            _context.AdminUsers.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
