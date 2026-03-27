using KacharaManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KacharaManagement.Repository.Interfaces
{
    public interface IAdminUserRepository
    {
        Task<AdminUser?> GetByUsernameAndPasswordAsync(string username, string password);
        Task<List<AdminUser>> GetAllAsync();
        Task AddAsync(AdminUser user);
    }
}
