using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Shared.RBAC;

namespace WMSSolution.Core.Services
{
    /// <summary>
    /// account service interface
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Audit User Action
        /// </summary>
        /// <param name="clientAction"></param>
        /// <returns></returns>
        Task AuditUserAction(ClientEnvironment clientAction);

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<BaseUserInfo> GetUserInfo(CurrentUser currentUser);

        /// <summary>
        /// login
        /// </summary>
        /// <param name="loginInput">user 's account infomation</param>
        /// <returns></returns>
        Task<LoginOutputViewModel> Login(LoginInputViewModel loginInput);
        /// <summary>
        /// Register
        /// </summary>
        /// <param name="registerInput"></param>
        /// <returns></returns>
        Task<bool> RegisterAsync(RegisterRequest registerInput);
    }
}
