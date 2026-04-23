using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Goodslocation;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;
using WMSSolution.WMS.Entities.ViewModels.Stock;
using WMSSolution.WMS.Entities.ViewModels.Stock.DuyPhatSolution;

namespace WMSSolution.WMS.IServices.Stock;

/// <summary>
/// Interface of StockService
/// </summary>
public interface IStockService : IBaseService<StockEntity>
{
    #region Api

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(List<StockManagementViewModel> data, int totals)> StockPageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// location stock page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(List<LocationStockManagementViewModel> data, int totals)> LocationStockPageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    ///  page search select
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(List<StockViewModel> data, int totals)> SelectPageAsync(PageSearch pageSearch, CurrentUser currentUser);

    /// <summary>
    /// sku page search select
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(List<SkuSelectViewModel> data, int totals)> SkuSelectPageAsync(PageSearch pageSearch, CurrentUser currentUser);

    /// <summary>
    /// sku page search select available
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    Task<(List<SkuSelectViewModel> data, int totals)> SkuSelectAvailablePageAsync(PageSearch pageSearch);

    /// <summary>
    /// safety stock
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(List<SafetyStockManagementViewModel> data, int totals)> SafetyStockPageAsync(PageSearch pageSearch, CurrentUser currentUser);

    /// <summary>
    /// get stock infomation by phone
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<List<LocationStockManagementViewModel>> LocationStockForPhoneAsync(LocationStockForPhoneSearchViewModel input, CurrentUser currentUser);

    /// <summary>
    /// delivery statistic
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(List<DeliveryStatisticViewModel> datas, int totals)> DeliveryStatistic(DeliveryStatisticSearchViewModel input, CurrentUser currentUser);


    /// <summary>
    /// stock age page search
    /// </summary>
    /// <param name="input">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(List<StockAgeViewModel> data, int totals)> StockAgePageAsync(StockAgeSearchViewModel input, CurrentUser currentUser);

    /// <summary>
    /// badge total data in stock
    /// </summary>
    /// <returns></returns>
    Task<InventoryDashboardViewModel> GetDashboardStatsAsync(CurrentUser currentUser);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(List<ProductLifeCycleViewModel> data, int totals)> GetStockProductTraceabilityAsync(PageSearch pageSearch, CurrentUser currentUser);

    #endregion Api

    #region Duy Phat Outbound

    /// <summary>
    /// Gets SKU available stock summary with pagination and search.
    /// Allows user to search SKU by name/code and see available quantity.
    /// </summary>
    /// <param name="pageSearch">Page search parameters with filters</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>Paginated list of SKU with available quantities</returns>
    Task<(List<SkuAvailableSummaryViewModel> data, int totals)> GetSkuAvailableSummaryAsync(
        PageSearch pageSearch,
        CurrentUser currentUser);

    /// <summary>
    /// Gets available stock grouped by Pallet and Location for a specific SKU.
    /// Used for manual outbound selection with FEFO (First Expiry First Out) ordering.
    /// </summary>
    /// <param name="skuId">SKU ID to query</param>
    /// <param name="requiredQty">Required quantity for auto-suggest</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>List of available stock options for user selection</returns>
    Task<List<AvailableStockSelectionViewModel>> GetAvailableStockForOutboundAsync(
        int skuId,
        decimal requiredQty,
        CurrentUser currentUser);

    #endregion Duy Phat Outbound

    /// <summary>
    /// Get a list of all locations that contain items matching skuID.
    /// </summary>
    /// <param name="request">Request containing SKU and warehouse information</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<List<LocationWithPalletViewModel>> GetLocationStockBySkuAsync(GetLocationStockBySkuRequest request,
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Filter Sku Location By Stock
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<GoodSkuLocationInfo>> FilterSkuLocationByStock(GetLocationStockBySkuRequest request,
        CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Get list Location DTO with stock 
    /// </summary>
    /// <param name="warehouseID"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<LocationStockInfoDTO>?> GetLocationStockInfoAsync(int warehouseID, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Upsert location sync conflict records based on the provided list of requests. This method will insert new records for conflicts that do not exist and update existing records for conflicts that already exist in the database
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(int inserted, int updated)> UpsertLocationSyncConflictsAsync(List<UpsertLocationSyncConflictRequest> requests, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Resolve logic for insert stock when conflict
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> ResolveWcsOnlyInboundAsync(
        ResolveWcsOnlyInboundRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolve logic for pallet merge in same location when conflict
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> ResolvePalletMergeSameLocationAsync(
    ResolvePalletMergeSameLocationRequest request,
    CurrentUser currentUser,
    CancellationToken cancellationToken);

    /// <summary>
    /// Resolve logic for clear location when conflict
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> ResolveWmsOnlyClearLocationAsync(
    ResolveWmsOnlyClearLocationRequest request,
    CurrentUser currentUser,
    CancellationToken cancellationToken);

    /// <summary>
    /// Resolve logic for location mismatch
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> ResolveLocationMismatchAsync(
    ResolveLocationMismatchRequest request,
    CurrentUser currentUser,
    CancellationToken cancellationToken);

    /// <summary>
    /// Get location sync logs by warehouse
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<LocationSyncLogItemDto>> GetLocationSyncLogsAsync(
        int warehouseId,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get location sync conflicts by traceId
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<LocationSyncConflictKeyDto>> GetLocationSyncConflictsByTraceIdAsync(
        string traceId,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cancel previous logs and delete conflicts async
    /// </summary>
    /// <param name="warehouseId">Warehouse ID</param>
    /// <param name="currentTraceId">Current trace ID</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<(int canceledLogs, int deletedConflicts)> CancelPreviousLogsAndDeleteConflictsAsync(
        int warehouseId,
        string currentTraceId,
        CurrentUser currentUser,
        CancellationToken cancellationToken);
}