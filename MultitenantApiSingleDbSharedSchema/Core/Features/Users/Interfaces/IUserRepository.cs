using MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUserResponse?> GetByIdAsync(Guid userId, string tenantId);
}