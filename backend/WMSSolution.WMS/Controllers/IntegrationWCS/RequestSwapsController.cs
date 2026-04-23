using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Swap;
using WMSSolution.WMS.IServices.IntegrationWCS;

namespace WMSSolution.WMS.Controllers.IntegrationWCS;

/// <summary>
/// WMS Request WCS Swaps pallets
/// </summary>
/// <param name="service"></param>
[Route("request-swaps")]
[ApiController]
[ApiExplorerSettings(GroupName = "WCS")]
[Authorize(Roles = "Integration")]
[Authorize]
public class RequestSwapsController(IIntegrationService service) : BaseController
{
    private readonly IIntegrationService _service = service;

    /// <summary>
    /// Reshuffling
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ResultModel<ReshufflingResponse>> Reshuffling()
    {
        var data = await _service.ReshufflingAsync(CurrentUser);
        return ResultModel<ReshufflingResponse>.Success(new ReshufflingResponse
        {
            Details = data,
            ResponseDate = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Update Status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("swaps/{id}")]
    public async Task<ResultModel<bool>> UpdateStatusTask(long id, [FromBody] ReshufflingRequest request)
    {
        if (id != request.SwapId)
        {
            return ResultModel<bool>.Error("Swap ID in URL does not match Swap ID in request body.", 400, false);
        }

        var data = await _service.UpdateReshufflingAsync(request.SwapId, request.Status, CurrentUser);
        return ResultModel<bool>.Success(data);
    }
}

