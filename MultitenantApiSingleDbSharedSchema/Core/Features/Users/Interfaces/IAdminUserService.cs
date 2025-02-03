using MultitenantApiSingleDbSharedSchema.Core.Common;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

public interface IAdminUserService
{
    Task<OperationResult> CreateAsync(
        string username,
        string password,
        string displayName,
        string? role);

    Task<IReadOnlyList<ApplicationUser>> GetAllAsync();

    Task<ApplicationUserResponse?> GetByIdAsync(Guid id);

    Task<OperationResult> DeleteAsync(Guid id);
}