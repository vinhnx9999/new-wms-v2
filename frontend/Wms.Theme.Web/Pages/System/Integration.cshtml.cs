using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wms.Theme.Web.Services.RBAC;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Pages.System;

public class IntegrationModel(IIntegrationService service) : PageModel
{
    private readonly IIntegrationService _service = service;

    [BindProperty(SupportsGet = true)]
    public string AppKey { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string ApiUrl { get; set; } = "";

    public async Task OnGetAsync()
    {
        var integrationInfo = await _service.GetIntegrationInfo();
        AppKey = integrationInfo.AppKey;
        ApiUrl = integrationInfo.ApiUrl;
    }

    public async Task<JsonResult> OnPostWCSIntegration([FromBody] IntegrationInfo request)
    {
        if (request is null || string.IsNullOrEmpty(request.AppKey) || string.IsNullOrEmpty(request.ApiUrl))
        {
            return new JsonResult(new { success = false, message = "Invalid request data" });
        }

        var (isSuccess, message) = await _service.UpdateIntegrationInfo(request);
        return new JsonResult(isSuccess
            ? new { success = true }
            : new { success = false, message = message ?? "Failed to update integration info" });
    }
}