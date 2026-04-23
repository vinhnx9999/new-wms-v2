using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Core.Utility;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound;
using WMSSolution.WMS.IServices.IntegrationWCS;

namespace WMSSolution.WMS.Controllers.IntegrationWCS;

/// <summary>
/// Controller for handling Inbound operations.
/// </summary>
/// <param name="service"></param>

[Route("inbound")]
[ApiController]
[ApiExplorerSettings(GroupName = "WCS")]
[Authorize(Roles = "Integration")]
[Authorize]
public class InboundController(IIntegrationService service) : BaseController
{
    private readonly IIntegrationService _service = service;

    /// <summary>
    /// Retrieves a list of outbound publish tasks.
    /// </summary>
    /// <remarks>Use this endpoint to obtain information about all current outbound publish tasks. The
    /// response includes task details suitable for client consumption.</remarks>
    /// <returns>A containing the details of outbound publish tasks.</returns>
    [HttpGet("tasks")]
    public async Task<ResultModel<IEnumerable<InboundTaskResponse>>> GetPublishTasks(CancellationToken cancellationToken)
    {
        var data = await _service.GetInboundTaskAsync(CurrentUser, cancellationToken) ?? [];
        return ResultModel<IEnumerable<InboundTaskResponse>>.Success(data);
    }

    /// <summary>
    /// Retrieves a list of inbound publish tasks by status.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Inbound Task </returns>
    /// <remarks>Use this endpoint to obtain information about inbound publish tasks filtered by status and date range.</remarks>
    [HttpPost("task-condition")]
    public async Task<ResultModel<IEnumerable<InboundTaskResponse>>> GetPublishTasksByStatus(InboundTaskConditionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetInboundTaskByStatusAsync(request, CurrentUser, cancellationToken) ?? [];
        return ResultModel<IEnumerable<InboundTaskResponse>>.Success(result);
    }

    /// <summary>
    /// Reject Inbound task
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>this endpoint using for reject task</remarks>
    [HttpPatch("reject")]
    public async Task<ResultModel<bool>> RejectTask([FromBody] InboundStatusRequest request, CancellationToken cancellationToken)
    {
        var header = Request.Headers[GlobalConsts.WCSKeyHeader].FirstOrDefault();
        var result = await _service.RejectInboundTaskAsync(request, header, CurrentUser, cancellationToken);
        if (result)
        {
            return ResultModel<bool>.Success(result, "Reject Task Successfully");
        }
        return ResultModel<bool>.Error("Reject Task Failed");
    }


    /// <summary>
    /// Updates the status of the specified inbound task.
    /// </summary>
    /// <remarks>If the task ID in the URL does not match the TaskId in the request body, the operation fails
    /// with a 400 error. This method is intended for partial updates to the task's status.</remarks>
    /// <param name="request">An object containing the new status information for the task. Must not be null and must have a valid TaskId and
    /// StatusName.</param>
    /// <param name="cancellationToken">cancelation tokem</param>
    /// <returns>A result model indicating whether the status update was successful.</returns>
    [HttpPatch("confirm")]
    public async Task<ResultModel<bool>> UpdateStatusTask([FromBody] InboundStatusRequest request, CancellationToken cancellationToken)
    {
        var header = Request.Headers[GlobalConsts.WCSKeyHeader].FirstOrDefault();
        var data = await _service.UpdateInboundSuccessStatusAsync(request, header, CurrentUser, cancellationToken);
        if (data)
        {
            return ResultModel<bool>.Success(data, "Update Status Successfully");
        }
        return ResultModel<bool>.Error("Update Status Failed");
    }

    /// <summary>
    /// Update Processing Inbound task
    /// </summary>
    /// <param name="request">inbound status request</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>update result</returns>
    [HttpPatch("processing")]
    public async Task<ResultModel<bool>> UpdateProcessingTask([FromBody] InboundStatusRequest request, CancellationToken cancellationToken)
    {
        var header = Request.Headers[GlobalConsts.WCSKeyHeader].FirstOrDefault();
        var data = await _service.UpdateInboundProcessingStatusAsync(request, header, CurrentUser, cancellationToken);
        if (data)
        {
            return ResultModel<bool>.Success(data, "Update Processing Successfully");
        }
        return ResultModel<bool>.Error("Update Processing Failed");
    }
}