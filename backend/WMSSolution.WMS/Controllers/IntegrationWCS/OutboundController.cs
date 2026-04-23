using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Core.Utility;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Outbound;
using WMSSolution.WMS.IServices.IntegrationWCS;

namespace WMSSolution.WMS.Controllers.IntegrationWCS;

/// <summary>
/// Controller for handling Outbound operations.
/// </summary>
/// <param name="service"></param>
[Route("outbound")]
[ApiController]
[ApiExplorerSettings(GroupName = "WCS")]
[Authorize(Roles = "Integration")]
[Authorize]
public class OutboundController(IIntegrationService service) : BaseController
{
    private readonly IIntegrationService _service = service;

    /// <summary>
    /// Retrieves the current outbound publish task details.    
    /// </summary>
    /// <returns>A <see cref="ResultModel{OutboundTaskResponse}"/> containing the outbound task details. If no tasks are
    /// available, the response will contain an empty collection.</returns>
    [HttpGet("tasks")]
    public async Task<ResultModel<IEnumerable<OutboundTaskResponse>>> GetPublishTasks(CancellationToken cancellationToken)
    {
        var data = await _service.GetOutboundTaskAsync(CurrentUser, cancellationToken) ?? [];
        return ResultModel<IEnumerable<OutboundTaskResponse>>.Success(data);
    }

    /// <summary>
    /// Retrieves a list of outbound publish tasks by status.
    /// </summary>
    /// <param name="request">Filter conditions including status and date range</param>
    /// <returns>Outbound tasks matching the conditions</returns>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>Use this endpoint to obtain information about outbound publish tasks filtered by status and date range.</remarks>
    [HttpPost("task-condition")]
    public async Task<ResultModel<IEnumerable<OutboundTaskResponse>>> GetPublishTasksByStatus(OutboundTaskConditionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetOutboundTaskByStatusAsync(request, CurrentUser, cancellationToken) ?? [];
        return ResultModel<IEnumerable<OutboundTaskResponse>>.Success(result);
    }

    /// <summary>
    /// Reject Outbound task
    /// </summary>
    /// <param name="request">The outbound status request containing task details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    /// <remarks>This endpoint is used for rejecting an outbound task</remarks>
    [HttpPatch("reject")]
    public async Task<ResultModel<bool>> RejectTask([FromBody] OutboundStatusRequest request, CancellationToken cancellationToken)
    {
        var header = Request.Headers[GlobalConsts.WCSKeyHeader].FirstOrDefault();
        var result = await _service.RejectOutboundTaskAsync(request, header, CurrentUser, cancellationToken);
        if (result)
        {
            return ResultModel<bool>.Success(result, "Reject Task Successfully");
        }
        return ResultModel<bool>.Error("Reject Task Failed");
    }

    /// <summary>
    /// Updates the status of the specified outbound task as confirmed/successful.
    /// </summary>
    /// <remarks>This method is intended for confirming task completion and updating the task's status.</remarks>
    /// <param name="request">An object containing the task information. Must have a valid TaskCode and PalletCode.</param>
    /// <param name="cancellationToken" >Cancellation token</param>
    /// <returns>A result model indicating whether the status update was successful.</returns>
    [HttpPatch("confirm")]
    public async Task<ResultModel<bool>> ConfirmTask([FromBody] OutboundStatusRequest request, CancellationToken cancellationToken)
    {
        var header = Request.Headers[GlobalConsts.WCSKeyHeader].FirstOrDefault();
        var data = await _service.UpdateOutboundSuccessStatusAsync(request, header, CurrentUser, cancellationToken);
        if (data)
        {
            return ResultModel<bool>.Success(data, "Update Status Successfully");
        }
        return ResultModel<bool>.Error("Update Status Failed");
    }

    /// <summary>
    /// Update status for outboundTask
    /// </summary>
    /// <param name="request">The outbound status request containing task details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    [HttpPatch("processing")]
    public async Task<ResultModel<bool>> UpdateProcessingTask([FromBody] OutboundStatusRequest request, CancellationToken cancellationToken)
    {
        var header = Request.Headers[GlobalConsts.WCSKeyHeader].FirstOrDefault();
        var data = await _service.UpdateOutboundProcessingStatusAsync(request, header, CurrentUser, cancellationToken);
        if (data)
        {
            return ResultModel<bool>.Success(data, "Update Processing Successfully");
        }
        return ResultModel<bool>.Error("Update Processing Failed");
    }
}
