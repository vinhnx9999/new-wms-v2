
using Wms.Theme.Web.Model.RBAC;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.RBAC;

public interface IUserService
{
    Task<UserDetailDTO> GetUserByIdAsync(int userId);
    Task<IEnumerable<UserDetailDTO>> GetAllUsersAsync();
    Task<(int? data, string? message)> CreateUserAsync(CreateUserRequest user);
    Task<(int? data, string? message)> DeactiveUserAsync(int userId);
    Task<(int? data, string? message)> ActiveUserAsync(int userId);
    //Task<bool> DeleteUserAsync(int userId);
    //Task<string> ResetPasswordAsync(int userId);
    Task<(int? data, string? message)> UpdateUserInfo(UserDetailDTO request);
    Task<(int? data, string? message)> ResetUserPasswordAsync(int userId);
}
