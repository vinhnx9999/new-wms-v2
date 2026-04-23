using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Core.Utility;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Swap;
using WMSSolution.WMS.IServices.IntegrationWCS;
using WMSSolution.WMS.IServices.Stock;

namespace WMSSolution.WMS.Controllers.IntegrationWCS;

/// <summary>
/// Swaps controller for handling swap-related operations with WCS.
/// </summary>
/// <param name="service"></param>
/// <param name="stockService"></param>
[Route("locations")]
[ApiController]
[ApiExplorerSettings(GroupName = "WCS")]
[Authorize]

public class LocationsController(IIntegrationService service,
                                  IStockService stockService) : BaseController
{
    private readonly IIntegrationService _service = service;
    private readonly IStockService _stockService = stockService;
    /// <summary>
    /// Get WCS Locations
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [HttpGet("map-locations/{blockId}")]
    public async Task<ResultModel<IEnumerable<LocationResponse>>> GetLocations(Guid? blockId = null, CancellationToken cancellation = default)
    {
        var data = await _service.GetMapLocations(blockId, CurrentUser, cancellation) ?? [];
        return ResultModel<IEnumerable<LocationResponse>>.Success(data);
    }

    /// <summary>
    /// Get Wcs Blocks
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [HttpGet("wcs-blocks")]
    public async Task<ResultModel<IEnumerable<BlockLocation>>> GetWcsBlocks(CancellationToken cancellation)
    {
        var data = await _service.GetBlockLocations(cancellation) ?? [];
        return ResultModel<IEnumerable<BlockLocation>>.Success(data);
    }
    /// <summary>
    /// Update Pallet Location
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Roles = "Integration")]
    [HttpPost("swaps")]
    public async Task<ResultModel<bool>> UpdatePalletLocation([FromBody] SwapPalletRequest request)
    {
        var data = await _service.UpdatePalletLocationAsync(
            request.PalletCode, request.FromLocation, request.ToLocation, CurrentUser);
        return ResultModel<bool>.Success(data);
    }

    /// <summary>
    /// Get pallet with location from wcs
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("pallet/{blockId}")]
    public async Task<ResultModel<List<PalletLocationDto>>> GetPalletLocation([FromRoute] string blockId, CancellationToken cancellationToken)
    {
        var result = await _service.GetPalletLocationAsync(blockId, cancellationToken) ?? [];
        return ResultModel<List<PalletLocationDto>>.Success(result);
    }

    /// <summary>
    /// Sync location data from WCS to WMS
    /// </summary>
    /// <param name="request"></param>
    /// <param name="sourceSystem"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = "Integration")]
    [HttpPost("sync-location-data")]
    public async Task<ResultModel<bool>> SyncLocationData(
        [FromBody] SyncLocationRequest request,
        [FromHeader(Name = GlobalConsts.WCSKeyHeader)] string? sourceSystem,
        CancellationToken cancellationToken)
    {
        var traceID = Guid.NewGuid().ToString("N");
        var (success, message) = await _service.SyncLocationDataAsync(request,
                                                                      traceID,
                                                                      sourceSystem,
                                                                      CurrentUser,
                                                                      cancellationToken);
        return success
            ? ResultModel<bool>.Success(true, message)
            : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Retrieves a list of location synchronization logs for the specified warehouse.
    /// </summary>
    /// <param name="warehouseId">The unique identifier of the warehouse for which to retrieve synchronization logs.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result model containing a list of location synchronization log items. The list is empty if no logs are found for the specified warehouse.</returns>
    [HttpGet("location-sync-logs")]
    public async Task<ResultModel<List<LocationSyncLogItemDto>>> GetLocationSyncLogs(
    [FromQuery] int warehouseId,
    CancellationToken cancellationToken)
    {
        var data = await _stockService.GetLocationSyncLogsAsync(warehouseId, CurrentUser, cancellationToken);
        return ResultModel<List<LocationSyncLogItemDto>>.Success(data);
    }

    /// <summary>
    /// Retrieves a list of location synchronization conflicts associated with the specified trace identifier.
    /// </summary>
    /// <param name="traceId">The unique identifier for the synchronization trace used to filter location conflicts. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result model containing a list of location synchronization conflict keys. The list is empty if no conflicts
    /// are found for the specified trace identifier.</returns>
    [HttpGet("location-sync-conflicts/{traceId}")]
    public async Task<ResultModel<List<LocationSyncConflictKeyDto>>> GetLocationSyncConflictsByTraceId(
        [FromRoute] string traceId,
        CancellationToken cancellationToken)
    {
        var data = await _stockService.GetLocationSyncConflictsByTraceIdAsync(traceId, CurrentUser, cancellationToken);
        return ResultModel<List<LocationSyncConflictKeyDto>>.Success(data);
    }

    /// <summary>
    /// cancel previous logs with the same traceId and delete conflicts when sync location data from WCS to WMS
    /// </summary>
    /// <param name="warehouseId">Warehouse id</param>
    /// <param name="traceId">TraceId của log đang chỉnh sửa</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("location-sync-logs/{traceId}/cancel-previous")]
    public async Task<ResultModel<object>> CancelPreviousLogsAndDeleteConflicts(
        [FromRoute] string traceId,
        [FromQuery] int warehouseId,
        CancellationToken cancellationToken)
    {
        if (warehouseId <= 0 || string.IsNullOrWhiteSpace(traceId))
        {
            return ResultModel<object>.Error("warehouseId hoặc traceId không hợp lệ.");
        }

        var (canceledLogs, deletedConflicts) =
            await _stockService.CancelPreviousLogsAndDeleteConflictsAsync(
                warehouseId,
                traceId,
                CurrentUser,
                cancellationToken);

        return ResultModel<object>.Success(new
        {
            traceId,
            warehouseId,
            canceledLogs,
            deletedConflicts
        }, "Cancel previous logs and delete conflicts success.");
    }
}

