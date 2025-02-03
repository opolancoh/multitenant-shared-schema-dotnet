using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultitenantApiSingleDbSharedSchema.Core.Common;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;
using MultitenantApiSingleDbSharedSchema.Domain.Constants;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Services;

public class AdminUserService : IAdminUserService
{
    private readonly string _currentTenantId;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;


    public AdminUserService(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _currentTenantId = currentUserService.TenantId;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<OperationResult> CreateAsync(
        string username,
        string password,
        string displayName,
        string? role)
    {
        if (!string.IsNullOrEmpty(role) && role != UserRoles.Admin)
        {
            return OperationResult.Failure(new List<OperationResultError>
                { new() { Code = "Role", Description = "Unauthorized to create this user." } });
        }

        var user = new ApplicationUser
        {
            TenantId = _currentTenantId,
            UserName = $"{username}@{_currentTenantId}",
            DisplayName = displayName
        };

        var userResult = await _userManager.CreateAsync(user, password);
        if (!userResult.Succeeded)
        {
            return OperationResult.Failure(ExtractErrors(userResult.Errors));
        }

        if (!string.IsNullOrEmpty(role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return OperationResult.Failure(ExtractErrors(roleResult.Errors));
            }
        }

        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users
            .Where(u => u.TenantId == _currentTenantId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ApplicationUserResponse?> GetByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id, _currentTenantId);
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var rowsAffected = await _userManager.Users
            .Where(u => u.Id == id && u.TenantId == _currentTenantId)
            .ExecuteDeleteAsync();

        return rowsAffected > 0
            ? OperationResult.Success()
            : OperationResult.Failure(new List<OperationResultError>
                { new() { Code = "_", Description = "User not found or you don't have permission to delete it." } });
    }

    private static List<OperationResultError> ExtractErrors(IEnumerable<IdentityError> errors) =>
        errors.Select(e => new OperationResultError { Code = e.Code, Description = e.Description }).ToList();
}