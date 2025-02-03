using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultitenantApiSingleDbSharedSchema.Core.Common;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Services;

public class SystemUserService : ISystemUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SystemUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<OperationResult> CreateAsync(
        string tenantId,
        string username,
        string password,
        string displayName,
        string? role)
    {
        var user = new ApplicationUser
        {
            TenantId = tenantId,
            UserName = $"{username}@{tenantId}",
            DisplayName = displayName
        };

        // Create user
        var userResult = await _userManager.CreateAsync(user, password);
        if (!userResult.Succeeded)
        {
            var errors = userResult.Errors.Select(e =>
                new OperationResultError
                {
                    Code = e.Code,
                    Description = e.Description
                }
            );
            return OperationResult.Failure(errors);
        }

        // Assign role if provided
        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e =>
                    new OperationResultError
                    {
                        Code = e.Code,
                        Description = e.Description
                    }
                );
                return OperationResult.Failure(errors);
            }
        }

        return OperationResult.Success();
    }


    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id)
    {
        return await _userManager.Users
            .Where(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        // Directly execute delete without loading full user entity
        var rowsAffected = await _userManager.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(); // EF Core 7+ feature

        return rowsAffected > 0
            ? OperationResult.Success()
            : OperationResult.Failure(new List<OperationResultError>
                { new() { Code = "_", Description = "User not found or you don't have permission to delete it." } });
    }
}