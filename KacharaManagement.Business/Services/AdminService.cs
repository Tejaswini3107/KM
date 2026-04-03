using KacharaManagement.Core.Entities;
using KacharaManagement.Core;
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

        public async Task<LogPageResponse> GetLogsAsync(int page = 1, int pageSize = 20, string? level = null, string? source = null, string? search = null)
        {
            return await _logRepo.GetPagedAsync(page, pageSize, level, source, search);
        }
    }
}
