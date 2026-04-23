using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.Core.Models;
using WMSSolution.Core.JWT;
using WMSSolution.Shared.RBAC;
using WMSSolution.WMS.Entities.ViewModels.User;

namespace WMSSolution.WMS.IServices.User;

/// <summary>
/// Interface of UserService
/// </summary>
public interface IUserService : IBaseService<userEntity>
{
    #region Api

    /// <summary>
    /// get select items
    /// </summary>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<List<FormSelectItem>> GetSelectItemsAsnyc(CurrentUser currentUser);

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(List<UserViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);

    /// <summary>
    /// Get all datas
    /// </summary>
    /// <returns></returns>
    Task<List<UserViewModel>> GetAllAsync(CurrentUser currentUser);

    /// <summary>
    /// Get a data by id
    /// </summary>
    /// <param name="id">primary key</param>
    /// <returns></returns>
    Task<UserViewModel> GetAsync(int id);

    /// <summary>
    /// add a new data
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(int id, string msg)> AddAsync(UserViewModel viewModel, CurrentUser currentUser);

    /// <summary>
    /// update a data
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateAsync(UserViewModel viewModel, CurrentUser currentUser);

    /// <summary>
    /// delete a data
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteAsync(int id, CurrentUser currentUser);

    /// <summary>
    /// import users by excel
    /// </summary>
    /// <param name="datas">excel datas</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ExcelAsync(List<UserExcelImportViewModel> datas, CurrentUser currentUser);

    /// <summary>
    /// reset password
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    Task<(bool, string)> ResetPwd(BatchOperationViewModel viewModel);

    /// <summary>
    /// change password
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ChangePwd(UserChangePwdViewModel viewModel);

    /// <summary>
    /// register a new tenant
    /// </summary>
    /// <param name="viewModel">viewModel</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> Register(RegisterViewModel viewModel);
    /// <summary>
    /// Get all users 
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<UserDetailDTO>> GetAllDetailsAsync(CurrentUser currentUser);
    /// <summary>
    /// Create User
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(int id, string msg)> CreateUser(CreateUserViewModel viewModel, CurrentUser currentUser);
    /// <summary>
    /// Active User
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ActiveUser(int userId, CurrentUser currentUser);
    /// <summary>
    /// Account has been intentionally disabled by an admin or system or creator.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeActiveUser(int userId, CurrentUser currentUser);
    /// <summary>
    /// Update User Info
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateUserInfo(int userId, UserDetailDTO request, CurrentUser currentUser);
    /// <summary>
    /// Reset User Password
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ResetUserPasswordAsync(int userId, CurrentUser currentUser);
    /// <summary>
    /// Change Password
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="username"></param>
    /// <param name="curPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    Task<(bool flag, string? msg)> ChangePassword(CurrentUser currentUser, string username, string curPassword, string newPassword);
    /// <summary>
    /// Get Integration WCS
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IntegrationWcsInfo> GetIntegrationWCSAsync(CurrentUser currentUser);
    /// <summary>
    /// Update Integration WCS
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<(bool isSuccess, string? message)> UpdateIntegrationWCS(CurrentUser currentUser, IntegrationWcsInfo request);

    #endregion Api
}