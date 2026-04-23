using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Model.RBAC;
using Wms.Theme.Web.Models;
using Wms.Theme.Web.Services.RBAC;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Pages.System;

public class UsersModel(IUserService service) : PageModel
{
    private readonly IUserService _service = service;

    [BindProperty(SupportsGet = true)]
    public IEnumerable<UserDetailDTO> Data { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Data = await _service.GetAllUsersAsync();
        return Page();
    }

    public async Task<JsonResult> OnPostAddUserName([FromBody] CreateUserRequest request)
    {
        if (request is null)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        var (id, message) = await _service.CreateUserAsync(request);
        if (id == 0)
        {
            return new JsonResult(new { success = false, message });
        }
        return new JsonResult(new { success = true, id });
    }

    public async Task<JsonResult> OnPostDeActiveUser([FromBody] ActiveUserRequest request)
    {
        if (request.UserId <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
        (int? data, string? message) = await _service.DeactiveUserAsync(request.UserId);
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to delete user" });
    }

    public async Task<JsonResult> OnPostActiveUser([FromBody] ActiveUserRequest request)
    {
        if (request.UserId <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
        (int? data, string? message) = await _service.ActiveUserAsync(request.UserId);
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to delete user" });
    }

    public async Task<JsonResult> OnPostUpdateUserInfo([FromBody] UserDetailDTO request)
    {
        if (request.Id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        (int? data, string? message) = await _service.UpdateUserInfo(request);
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to delete user" });
    }

    public async Task<JsonResult> OnPostResetUserPassword([FromBody] BodyBaseRequest request)
    {
        if (request.Id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }
        (int? data, string? message) = await _service.ResetUserPasswordAsync(request.Id);
        return new JsonResult(data.HasValue && data.Value > 0
           ? new { success = true, id = data.Value }
           : new { success = false, message = message ?? "Failed to reset password" });
    }
}
