using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Goodslocation;
using WMSSolution.WMS.Entities.ViewModels.Stock;
using WMSSolution.WMS.Entities.ViewModels.Stock.DuyPhatSolution;
using WMSSolution.WMS.IServices.Stock;

namespace WMSSolution.WMS.Controllers;

/// <summary>
/// stock controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="stockService">stock Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("stock")]
[ApiController]
[ApiExplorerSettings(GroupName = "WMS")]
public class StockController(IStockService stockService,
                             IStringLocalizer<MultiLanguage> stringLocalizer) : BaseController
{
    #region Args

    /// <summary>
    /// stock Service
    /// </summary>
    private readonly IStockService _stockService = stockService;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion Args

    #region Api

    /// <summary>
    /// stock details page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("stock-list")]
    public async Task<ResultModel<PageData<StockManagementViewModel>>> StockPageAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _stockService.StockPageAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<StockManagementViewModel>>.Success(new PageData<StockManagementViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// safety stock page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("safety-list")]
    public async Task<ResultModel<PageData<SafetyStockManagementViewModel>>> SafetyStockPageAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _stockService.SafetyStockPageAsync(pageSearch, CurrentUser);

        return ResultModel<PageData<SafetyStockManagementViewModel>>.Success(new PageData<SafetyStockManagementViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// location stock page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("location-list")]
    public async Task<ResultModel<PageData<LocationStockManagementViewModel>>> LocationStockPageAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _stockService.LocationStockPageAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<LocationStockManagementViewModel>>.Success(new PageData<LocationStockManagementViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }


    /// <summary>
    /// page search select
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("select")]
    public async Task<ResultModel<PageData<StockViewModel>>> SelectPageAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _stockService.SelectPageAsync(pageSearch, CurrentUser);

        return ResultModel<PageData<StockViewModel>>.Success(new PageData<StockViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// sku page search select
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("sku-select")]
    public async Task<ResultModel<PageData<SkuSelectViewModel>>> SkuSelectPageAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _stockService.SkuSelectPageAsync(pageSearch, CurrentUser);

        return ResultModel<PageData<SkuSelectViewModel>>.Success(new PageData<SkuSelectViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// sku page search select available
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("sku-select-available")]
    public async Task<ResultModel<PageData<SkuSelectViewModel>>> SkuSelectAvailablePageAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _stockService.SkuSelectAvailablePageAsync(pageSearch);

        return ResultModel<PageData<SkuSelectViewModel>>.Success(new PageData<SkuSelectViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// get stock infomation by phone
    /// </summary>
    /// <param name="input">input</param>
    /// <returns></returns>
    [HttpPost("qrcode-list")]
    public async Task<ResultModel<List<LocationStockManagementViewModel>>> LocationStockForPhoneAsync(LocationStockForPhoneSearchViewModel input)
    {
        var datas = await _stockService.LocationStockForPhoneAsync(input, CurrentUser);
        return ResultModel<List<LocationStockManagementViewModel>>.Success(datas);
    }

    /// <summary>
    /// delivery statistic
    /// </summary>
    /// <param name="input">input</param>
    /// <returns></returns>
    [HttpPost("delivery-list")]
    public async Task<ResultModel<PageData<DeliveryStatisticViewModel>>> LocationStockForPhoneAsync(DeliveryStatisticSearchViewModel input)
    {
        var (data, totals) = await _stockService.DeliveryStatistic(input, CurrentUser);
        return ResultModel<PageData<DeliveryStatisticViewModel>>.Success(new PageData<DeliveryStatisticViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }



    /// <summary>
    /// stock age page search
    /// </summary>
    /// <returns></returns>
    [HttpPost("stock-age-list")]
    public async Task<ResultModel<PageData<StockAgeViewModel>>> StockAgePageAsync(StockAgeSearchViewModel input)
    {
        var (data, totals) = await _stockService.StockAgePageAsync(input, CurrentUser);

        return ResultModel<PageData<StockAgeViewModel>>.Success(new PageData<StockAgeViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }
    #endregion Api

    #region New Flow Api 8/12/2025
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("dashboard-stats")]
    public async Task<ResultModel<InventoryDashboardViewModel>> GetDashboardStatsAsync()
    {
        var data = await _stockService.GetDashboardStatsAsync(CurrentUser);

        return data != null ? ResultModel<InventoryDashboardViewModel>.Success(data) :
                ResultModel<InventoryDashboardViewModel>.Error(_stringLocalizer["DataNotFound"]);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    [HttpPost("product-lifecycle")]
    public async Task<ResultModel<PageData<ProductLifeCycleViewModel>>> GetProductLifecycleAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _stockService.GetStockProductTraceabilityAsync(pageSearch, CurrentUser);
        return ResultModel<PageData<ProductLifeCycleViewModel>>.Success(new PageData<ProductLifeCycleViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    #endregion

    #region Duy Phat Outbound

    /// <summary>
    /// Gets SKU available stock summary with pagination and search.
    /// Allows user to search SKU by name/code and see available quantity.
    /// </summary>
    /// <param name="pageSearch">Page search parameters with filters for sku_name, sku_code, spu_name...</param>
    /// <returns>Paginated list of SKU with available quantities</returns>
    [HttpPost("sku-available")]
    public async Task<ResultModel<PageData<SkuAvailableSummaryViewModel>>> GetSkuAvailableSummaryAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _stockService.GetSkuAvailableSummaryAsync(pageSearch, CurrentUser);
        return ResultModel<PageData<SkuAvailableSummaryViewModel>>.Success(new PageData<SkuAvailableSummaryViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// Gets available stock grouped by Pallet and Location for a specific SKU.
    /// Used for manual outbound selection with FEFO (First Expiry First Out) ordering.
    /// </summary>
    /// <param name="skuId">SKU ID to query</param>
    /// <param name="requiredQty">Required quantity for auto-suggest pick</param>
    /// <returns>List of available stock options (Pallet, Location) with suggested quantities</returns>
    [HttpGet("available-for-outbound")]
    public async Task<ResultModel<List<AvailableStockSelectionViewModel>>> GetAvailableStockForOutboundAsync(
        [FromQuery] int skuId,
        [FromQuery] decimal requiredQty)
    {
        if (skuId <= 0)
        {
            return ResultModel<List<AvailableStockSelectionViewModel>>.Error(_stringLocalizer["InvalidSkuId"]);
        }

        if (requiredQty <= 0)
        {
            return ResultModel<List<AvailableStockSelectionViewModel>>.Error(_stringLocalizer["InvalidQuantity"]);
        }

        var data = await _stockService.GetAvailableStockForOutboundAsync(skuId, requiredQty, CurrentUser);
        return ResultModel<List<AvailableStockSelectionViewModel>>.Success(data);
    }

    #endregion Duy Phat Outbound

    /// <summary>
    /// Get a list of all locations that contain items matching skuID.
    /// required warehouseID , skuId
    /// </summary>
    /// <param name="warehouseId">warehouseID</param>
    /// <param name="skuID">SKU ID</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    [HttpGet("location-by-sku")]
    public async Task<ResultModel<List<LocationWithPalletViewModel>>> LocationStockAllSkuAsync([FromQuery] int warehouseId, [FromQuery] int skuID, CancellationToken cancellationToken)
    {
        var request = new GetLocationStockBySkuRequest { WarehouseId = warehouseId, SkuId = skuID };
        var result = await _stockService.GetLocationStockBySkuAsync(request, CurrentUser, cancellationToken);
        return ResultModel<List<LocationWithPalletViewModel>>.Success(result);
    }

    /// <summary>
    /// Filter Sku Locations by stock availability.
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="skuID"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("filter-location-by-sku")]
    public async Task<ResultModel<List<GoodSkuLocationInfo>>> FilterSkuLocationByStock([FromQuery] int warehouseId, [FromQuery] int skuID, CancellationToken cancellationToken)
    {
        var request = new GetLocationStockBySkuRequest { WarehouseId = warehouseId, SkuId = skuID };
        var result = await _stockService.FilterSkuLocationByStock(request, CurrentUser, cancellationToken);
        return ResultModel<List<GoodSkuLocationInfo>>.Success(result);
    }


    #region stock count

    /// <summary>
    /// Get all location with pallet Code and item in this location
    /// </summary>
    ///<param name="warehouseID"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("get-location-stock-info/{warehouseID}")]
    public async Task<ResultModel<List<LocationStockInfoDTO?>>> GetLocationStockInfoAsync([FromRoute] int warehouseID, CancellationToken cancellationToken)
    {
        var result = await _stockService.GetLocationStockInfoAsync(warehouseID, CurrentUser, cancellationToken) ?? [];
        return ResultModel<List<LocationStockInfoDTO?>>.Success(result);
    }

    /// <summary>
    /// Update Insert conflict location sync data, this API is used for WCS to sync location data to WMS
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("upsert-location-sync-conflicts")]
    public async Task<ResultModel<int>> UpsertLocationSyncConflictsAsync([FromBody] List<UpsertLocationSyncConflictRequest> requests, CancellationToken cancellationToken)
    {
        var (inserted, updated) = await _stockService.UpsertLocationSyncConflictsAsync(requests, CurrentUser, cancellationToken);

        return ResultModel<int>.Success(inserted + updated);
    }

    /// <summary>
    /// Insert stock and update conflict
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("resolve-wcs-only-inbound")]
    public async Task<ResultModel<bool>> ResolveWcsOnlyInboundAsync(
    [FromBody] ResolveWcsOnlyInboundRequest request,
    CancellationToken cancellationToken)
    {
        var (success, message) = await _stockService.ResolveWcsOnlyInboundAsync(
            request, CurrentUser, cancellationToken);

        return success
            ? ResultModel<bool>.Success(true)
            : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Merge conflic
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("resolve-pallet-merge-same-location")]
    public async Task<ResultModel<bool>> ResolvePalletMergeSameLocationAsync(
    [FromBody] ResolvePalletMergeSameLocationRequest request,
    CancellationToken cancellationToken)
    {
        var (success, message) = await _stockService.ResolvePalletMergeSameLocationAsync(
            request, CurrentUser, cancellationToken);

        return success
            ? ResultModel<bool>.Success(true)
            : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Handle for wms only
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    [HttpPost("resolve-wms-only-clear-location")]
    public async Task<ResultModel<bool>> ResolveWmsOnlyClearLocationAsync(
    [FromBody] ResolveWmsOnlyClearLocationRequest request,
    CancellationToken cancellationToken)
    {
        var (success, message) = await _stockService.ResolveWmsOnlyClearLocationAsync(
            request, CurrentUser, cancellationToken);

        return success
            ? ResultModel<bool>.Success(true)
            : ResultModel<bool>.Error(message);
    }

    /// <summary>
    /// Resolve location mismatch between WMS and WCS
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("resolve-location-mismatch")]
    public async Task<ResultModel<bool>> ResolveLocationMismatchAsync(
        [FromBody] ResolveLocationMismatchRequest request,
        CancellationToken cancellationToken)
    {
        var (success, message) = await _stockService.ResolveLocationMismatchAsync(
            request, CurrentUser, cancellationToken);

        return success
            ? ResultModel<bool>.Success(true)
            : ResultModel<bool>.Error(message);
    }

    #endregion stock count
}
