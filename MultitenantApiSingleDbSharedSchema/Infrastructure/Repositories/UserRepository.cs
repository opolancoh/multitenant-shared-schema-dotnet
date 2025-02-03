using Microsoft.EntityFrameworkCore;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;
using MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

namespace MultitenantApiSingleDbSharedSchema.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUserResponse?> GetByIdAsync(Guid userId, string tenantId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId && x.TenantId == tenantId)
            .Select(x => new ApplicationUserResponse
            {
                Id = x.Id,
                UserName = x.UserName!,
                DisplayName = x.DisplayName,
                Email = x.Email,
                TenantId = x.TenantId,
                Roles = _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(_context.Roles,
                        userRole => userRole.RoleId,
                        role => role.Id,
                        (userRole, role) => role.Name!)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}