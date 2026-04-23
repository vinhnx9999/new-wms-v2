using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.ViewModels.Dashboard;
using WMSSolution.WMS.IServices;
using WMSSolution.WMS.IServices.Customer;
using WMSSolution.WMS.IServices.OutboundGateway;
using WMSSolution.WMS.IServices.Receipt;
using WMSSolution.WMS.IServices.Sku;
using WMSSolution.WMS.IServices.Warehouse;

namespace WMSSolution.WMS.Controllers.Dashboard;

/// <summary>
/// Dashboard
/// </summary>
[Route("dashboard")]
[ApiController]
[ApiExplorerSettings(GroupName = "WMS")]
public class DashboardController(IWarehouseService warehouseService, 
    ISkuService skuService, IReceiptService receiptService,
    IOutboundGatewayService outboundService,
    ISupplierService supplierService,
    ICustomerService customerService) : BaseController
{
    private readonly ISkuService _skuService = skuService;
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly IReceiptService _receiptService = receiptService;
    private readonly IOutboundGatewayService _outboundService = outboundService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ISupplierService _supplierService = supplierService;

    /// <summary>
    /// Load Master Data
    /// </summary>
    /// <returns></returns>
    [HttpGet("master-data")]
    public async Task<ResultModel<MasterDataDto>> LoadMasterData()
    {
        var info = new MasterDataDto
        {
            Locations = await _warehouseService.GetMasterData(CurrentUser),
            Skus = await _skuService.GetMasterData(CurrentUser),
            Customers = await _customerService.GetMasterData(CurrentUser),
            Suppliers = await _supplierService.GetMasterData(CurrentUser)
        };

        return ResultModel<MasterDataDto>.Success(info);        
    }

    /// <summary>
    /// Dashboard Info
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ResultModel<DashboardInfo>> GetDashboardInfo([FromQuery] FilterDashboardByTime filterDashboard = FilterDashboardByTime.LastWeek)
    {
        var inventory = await _warehouseService.GetWarehouseInfo(CurrentUser);
        var totalItems = await _skuService.GetTotalItems(CurrentUser);
        var receipts = await _receiptService.GetInboundInfo(CurrentUser);

        var pendingStatuses = new[]
        {
            ReceiptStatus.NEW,
            ReceiptStatus.DRAFT,
            ReceiptStatus.CANCELED,
        };

        var pending = receipts.Where(x => pendingStatuses.Contains(x.ItemStatus));

        var processingStatuses = new[]
        {
            ReceiptStatus.NEW,
            ReceiptStatus.DRAFT,
            ReceiptStatus.PROCESSING
        };

        var processing = receipts.Where(x => processingStatuses.Contains(x.ItemStatus));

        var finishStatuses = new[]
        {
            ReceiptStatus.COMPLETE
        };

        DateTime today = DateTime.UtcNow.Date;
        var fromDate = filterDashboard switch
        {
            FilterDashboardByTime.LastWeek => today.AddDays(-7),
            FilterDashboardByTime.LastMonth => today.AddMonths(-1),
            FilterDashboardByTime.Last3Month => today.AddMonths(-3),
            _ => today.AddDays(-7)
        };

        var inboundItems = await _receiptService.GetInboundByRangeDate(CurrentUser, finishStatuses, fromDate, today);
        var outboundItems = await _outboundService.GetOutboundByRangeDate(CurrentUser, finishStatuses, fromDate, today);

        var finishToday = await _receiptService.GetInboundByDate(CurrentUser, finishStatuses, today);
        var finishYesterday = await _receiptService.GetInboundByDate(CurrentUser, finishStatuses, today.AddDays(-1));

        var info = new DashboardInfo
        {
            TotalWarehouses = inventory.TotalWarehouses,
            TotalItems = totalItems,
            TotalInventory = inventory.TotalLocations,
            WarehouseCapacity = inventory.WarehouseCapacity,
            Items = receipts,
            PendingOrders = (pending ?? []).Sum(x => x.TotalCount),
            TodayOrders = (finishToday ?? []).Sum(x => x.TotalCount),
            YesterdayOrders = (finishYesterday ?? []).Sum(x => x.TotalCount),
            ProcessingOrders = (processing ?? []).Sum(x => x.TotalCount),
            LowInventoryAlert = inventory.LowInventoryAlert,
            InboundItems = inboundItems,
            OutboundItems = outboundItems,
        };

        return ResultModel<DashboardInfo>.Success(info);
    }
}
