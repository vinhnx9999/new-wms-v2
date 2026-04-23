using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.GoodLocation;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Stock;

namespace Wms.Theme.Web.Services.Stock;

public interface IStockService
{
    Task<List<SkuSelectDTO>> GetSkuSelect(PageSearchRequest pageSearchRequest);
    Task<List<SkuSelectDTO>> GetSkuSelectAvailable(PageSearchRequest pageSearchRequest);
    Task<List<StockLocationDTO>> GetStockLocationsAsync();
    Task<ResultModel<PageData<LocationStockManagementViewModel>>> LocationStockPageAsync(PageSearchRequest request);
    Task<ResultModel<InventoryDashboardViewModel>> GetInventoryDashboardAsync();
    Task<ResultModel<PageData<StockViewModel>>> GetSelectPageAsync(PageSearchRequest pageSearchRequest);
    Task<ResultModel<PageData<ProductLifeCycleViewModel>>> GetProductLiftPageAsync(PageSearchRequest pageSearchRequest);
    Task<ResultModel<PageData<StockManagementViewModel>>> StockPageAsync(PageSearchRequest request);

    /// <summary>
    /// Search SKU with available quantity for outbound
    /// </summary>
    Task<ResultModel<PageData<SkuAvailableDTO>>> GetSkuAvailableAsync(PageSearchRequest request);

    /// <summary>
    /// Get available pallets/locations for outbound with FEFO suggestion
    /// </summary>
    Task<ResultModel<List<PalletAvailableDTO>>> GetAvailableForOutboundAsync(int skuId, decimal requiredQty);
    Task<List<LocationWithPalletViewModel>> GetLocationStockBySku(GetLocationStockBySkuRequest request);
    Task<List<GoodSkuLocationInfo>> FilterSkuLocationStock(GetLocationStockBySkuRequest request);

    /// <summary>
    /// Get all location stock info for a warehouse, including empty locations
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    Task<List<LocationStockInfoDTO>> GetLocationStockInfoAsync(string warehouseId);

    /// <summary>
    /// Update insert conflict records for location sync, which will be used for conflict management and reporting. The conflict happens when WMS and WCS have different data for the same location, which may cause issue for inventory management and outbound process. By upserting the conflict records, we can keep track of the conflicts and take necessary actions to resolve them.
    /// </summary>
    /// <param name="requests"></param>
    /// <returns></returns>
    Task<bool> UpsertLocationSyncConflictsAsync(List<UpsertLocationSyncConflictRequest> requests);

    /// <summary>
    /// adding for the conflict resolution of WCS only inbound
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<bool> ResolveWcsOnlyInboundAsync(ResolveWcsOnlyInboundRequest request);

    /// <summary>
    /// Resolve the pallet merge with same location conflict, which happens when two pallets are merged into one pallet at the same location, and WMS and WCS have different data for the pallets and location
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<bool> ResolvePalletMergeSameLocationAsync(ResolvePalletMergeSameLocationRequest request);

    /// <summary>
    /// Resolve the wms only clear location conflict, which happens when a location is cleared in WMS but not in WCS, causing a mismatch in the inventory records. This may affect the accuracy of inventory tracking and management in WMS.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<bool> ResolveWmsOnlyClearLocationAsync(ResolveWmsOnlyClearLocationRequest request);
    Task<bool> ResolveLocationMismatchAsync(ResolveLocationMismatchRequest request);

    /// <summary>
    /// Get log
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    Task<List<LocationSyncLogItemDto>> GetLocationSyncLogsAsync(int warehouseId);

    /// <summary>
    /// Get conflic by log
    /// </summary>
    /// <param name="traceId"></param>
    /// <returns></returns>
    Task<List<LocationSyncConflictKeyDto>> GetLocationSyncConflictsByTraceIdAsync(string traceId);

    /// <summary>
    /// Cancel Previous Logs And Delete Conflicts, which will be used when the WCS data is corrected and we want to cancel the previous logs and delete the conflicts for better data accuracy and inventory management. By canceling the previous logs and deleting the conflicts, we can ensure that the inventory records in WMS are consistent with the corrected data in WCS
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="traceId"></param>
    /// <returns></returns>
    Task<(bool success, int canceledLogs, int deletedConflicts, string message)>
        CancelPreviousLogsAndDeleteConflictsAsync(int warehouseId, string traceId);
}
