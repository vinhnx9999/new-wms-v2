
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.RBAC;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.User;
using WMSSolution.WMS.IServices.User;

namespace WMSSolution.WMS.Controllers.User;

/// <summary>
/// user controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="userService">user Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("user")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class UserController(
     IUserService userService
       , IStringLocalizer<MultiLanguage> stringLocalizer
         ) : BaseController
{
    #region Args

    /// <summary>
    /// user Service
    /// </summary>
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion Args

    #region Api

    /// <summary>
    /// get select items
    /// </summary>
    /// <returns></returns>
    [HttpGet("select-item")]
    public async Task<ResultModel<List<FormSelectItem>>> GetSelectItemsAsnyc()
    {
        var datas = await _userService.GetSelectItemsAsnyc(CurrentUser);
        return ResultModel<List<FormSelectItem>>.Success(datas);
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<UserViewModel>>> PageAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _userService.PageAsync(pageSearch, CurrentUser);

        return ResultModel<PageData<UserViewModel>>.Success(new PageData<UserViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns>args</returns>
    [HttpGet("all")]
    public async Task<ResultModel<List<UserViewModel>>> GetAllAsync()
    {
        var data = await _userService.GetAllAsync(CurrentUser);
        if (data.Count != 0)
        {
            return ResultModel<List<UserViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<UserViewModel>>.Success([]);
        }
    }

    /// <summary>
    /// Get All users of current tenant with details
    /// </summary>
    /// <returns></returns>
    [HttpGet("user-details")]
    public async Task<ResultModel<IEnumerable<UserDetailDTO>>> GetAllDetailsAsync()
    {
        var data = await _userService.GetAllDetailsAsync(CurrentUser);
        if (data.Any())
        {
            return ResultModel<IEnumerable<UserDetailDTO>>.Success(data ?? []);
        }
        else
        {
            return ResultModel<IEnumerable<UserDetailDTO>>.Success([]);
        }
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns>args</returns>
    [HttpGet]
    public async Task<ResultModel<UserViewModel>> GetAsync(int id)
    {
        var data = await _userService.GetAsync(id);
        if (data != null)
        {
            return ResultModel<UserViewModel>.Success(data);
        }
        else
        {
            return ResultModel<UserViewModel>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<int>> AddAsync(UserViewModel viewModel)
    {
        viewModel.creator = CurrentUser.user_name;
        var (id, msg) = await _userService.AddAsync(viewModel, CurrentUser);
        if (id > 0)
        {
            return ResultModel<int>.Success(id, msg);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// Create User
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    [HttpPost("create-user")]
    public async Task<ResultModel<int>> CreateUser(CreateUserViewModel viewModel)
    {
        var (id, msg) = await _userService.CreateUser(viewModel, CurrentUser);
        if (id > 0)
        {
            return ResultModel<int>.Success(id, msg);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// register a new tenant
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ResultModel<string>> Register(RegisterViewModel viewModel)
    {
        var (flag, msg) = await _userService.Register(viewModel);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// import users by excel
    /// </summary>
    /// <param name="excel_datas">excel datas</param>
    /// <returns></returns>
    [HttpPost("excel")]
    public async Task<ResultModel<string>> ExcelAsync(List<UserExcelImportViewModel> excel_datas)
    {
        var (flag, msg) = await _userService.ExcelAsync(excel_datas, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultModel<bool>> UpdateAsync(UserViewModel viewModel)
    {
        var (flag, msg) = await _userService.UpdateAsync(viewModel, CurrentUser);
        if (flag)
        {
            return ResultModel<bool>.Success(flag);
        }
        else
        {
            return ResultModel<bool>.Error(msg, 400, flag);
        }
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ResultModel<string>> DeleteAsync(int id)
    {
        var (flag, msg) = await _userService.DeleteAsync(id, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// reset password
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    [HttpPost("reset-pwd")]
    public async Task<ResultModel<string>> ResetPwd(BatchOperationViewModel viewModel)
    {
        var (flag, msg) = await _userService.ResetPwd(viewModel);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// change password
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    [HttpPost("change-pwd")]
    public async Task<ResultModel<string>> ChangePwd(UserChangePwdViewModel viewModel)
    {
        var (flag, msg) = await _userService.ChangePwd(viewModel);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// Active User
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPatch("{userId}/active")]
    public async Task<ResultModel<string>> ActiveUser(int userId)
    {
        var (flag, msg) = await _userService.ActiveUser(userId, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// De-Active User
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPatch("{userId}/de-active")]
    public async Task<ResultModel<string>> DeActiveUser(int userId)
    {
        var (flag, msg) = await _userService.DeActiveUser(userId, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// Reset User Password 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPatch("{userId}/reset-password")]
    public async Task<ResultModel<string>> ResetUserPassword(int userId)
    {
        var (flag, msg) = await _userService.ResetUserPasswordAsync(userId, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// Update User Info
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{userId}/update-user")]
    public async Task<ResultModel<string>> UpdateUserInfo(int userId, [FromBody] UserDetailDTO request)
    {
        var (flag, msg) = await _userService.UpdateUserInfo(userId, request, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// Change Password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>

    [HttpPatch("change-password")]
    public async Task<ResultModel<int>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var (flag, msg) = await _userService.ChangePassword(CurrentUser, request.Username, request.Password, request.NewPassword);
        if (flag)
        {
            return ResultModel<int>.Success(1);
        }
        else
        {
            return ResultModel<int>.Error(msg ?? "Error when change pass");
        }
    }

    /// <summary>
    /// Get Integration WCS 
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "SysAdmin,Admin")]
    [HttpGet("integration-wcs")]
    public async Task<ResultModel<IntegrationWcsInfo>> GetIntegrationWCSAsync()
    {
        var data = await _userService.GetIntegrationWCSAsync(CurrentUser);
        if (data != null)
        {
            return ResultModel<IntegrationWcsInfo>.Success(data);
        }
        else
        {
            return ResultModel<IntegrationWcsInfo>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    /// <summary>
    /// Update Integration
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "SysAdmin,Admin")]
    [HttpPatch("integration-wcs")]
    public async Task<ResultModel<int>> UpdateIntegrationWCS([FromBody] IntegrationWcsInfo request)
    {
        var (isSuccess, message) = await _userService.UpdateIntegrationWCS(CurrentUser, request);
        if (isSuccess)
        {
            return ResultModel<int>.Success(1);
        }
        else
        {
            return ResultModel<int>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    #endregion Api
}