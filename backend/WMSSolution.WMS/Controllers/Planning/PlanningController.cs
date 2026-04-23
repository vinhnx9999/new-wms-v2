using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels.Planning;
using WMSSolution.WMS.IServices.Planning;

namespace WMSSolution.WMS.Controllers.Planning;

/// <summary>
/// Planning
/// </summary>
/// <param name="service"></param>
/// <param name="stringLocalizer"></param>
[Route("planning")]
[ApiController]
[ApiExplorerSettings(GroupName = "WMS")]
public class PlanningController(IPlanningService service,
    IStringLocalizer<MultiLanguage> stringLocalizer) : BaseController
{
    /// <summary>
    /// pallet service
    /// </summary>
    private readonly IPlanningService _service = service;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;


    //  private readonly

    /// <summary>
    /// Get Picking List
    /// </summary>
    /// <returns></returns>
    [HttpGet("picking")]
    public async Task<ResultModel<IEnumerable<PickingDTO>>> GetPickingList()
    {
        var data = await _service.GetPickingList(CurrentUser);
        if (data.Any())
        {
            return ResultModel<IEnumerable<PickingDTO>>.Success(data);
        }

        return ResultModel<IEnumerable<PickingDTO>>.Success([]);
    }

    /// <summary>
    /// Get Planning Packing List
    /// </summary>
    /// <returns></returns>
    [HttpGet("packing")]
    public async Task<ResultModel<IEnumerable<PickingDTO>>> GetPackingList()
    {
        var data = await _service.GetPlanningPackingList(CurrentUser);
        if (data.Any())
        {
            return ResultModel<IEnumerable<PickingDTO>>.Success(data);
        }

        return ResultModel<IEnumerable<PickingDTO>>.Success([]);
    }

    /// <summary>
    /// Save Picking List
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("picking")]
    public async Task<ResultModel<bool>> SavePickingList([FromBody] IEnumerable<PickingDTO> requests, CancellationToken cancellationToken)
    {
        var result = await _service.SavePickingList(requests, CurrentUser, cancellationToken);
        if (!result.Success)
        {
            return ResultModel<bool>.Error(result.Message ?? "Save Picking error");
        }
        return ResultModel<bool>.Success(true);
    }
}

