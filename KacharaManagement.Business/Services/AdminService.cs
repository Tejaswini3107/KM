using KacharaManagement.Core.Entities;
using KacharaManagement.Business.Interfaces;
using KacharaManagement.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Business.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminUserRepository _adminRepo;
        private readonly ILogEntryRepository _logRepo;
        public AdminService(IAdminUserRepository adminRepo, ILogEntryRepository logRepo)
        {
            _adminRepo = adminRepo;
            _logRepo = logRepo;
        }


        public async Task<AdminUser?> LoginAsync(string username, string password)
        {
            return await _adminRepo.GetByUsernameAndPasswordAsync(username, password);
        }

        public async Task<bool> CreateAdminUserAsync(AdminUser user)
        {
            // Check if user exists
            var exists = (await _adminRepo.GetAllAsync()).Any(x => x.Username == user.Username);
            if (exists || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.PasswordHash))
                return false;
            user.CreatedAt = DateTime.UtcNow;
            await _adminRepo.AddAsync(user);
            return true;
        }

        public async Task<List<LogEntry>> GetLogsAsync(int limit = 100)
        {
            return await _logRepo.GetAllAsync(limit);
        }
    }
}
