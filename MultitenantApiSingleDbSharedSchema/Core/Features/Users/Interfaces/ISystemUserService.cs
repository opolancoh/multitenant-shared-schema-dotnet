using MultitenantApiSingleDbSharedSchema.Core.Common;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

public interface ISystemUserService
{
    Task<OperationResult> CreateAsync(
        string tenantId,
        string username,
        string password,
        string displayName,
        string? role);

    Task<IReadOnlyList<ApplicationUser>> GetAllAsync();

    Task<ApplicationUser?> GetByIdAsync(Guid id);
    
    Task<OperationResult> DeleteAsync(Guid id);
}