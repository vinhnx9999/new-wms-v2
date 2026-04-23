using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.Authen;

public interface IAuthenService
{
    Task AuditUserAction(ClientEnvironment environment);
    Task<(int id, string message)> ChangePassword(string userName, string password, string newPassword);
    Task<LoginResponse> LoginAsync(string username, string password, ClientEnvironment? environment = null);
    Task<bool> RegisterAsync(RegisterRequest request);
}