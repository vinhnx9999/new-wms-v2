using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.ViewModels.Reports;
using WMSSolution.WMS.IServices.Reports;

namespace WMSSolution.WMS.Controllers.Reports;

/// <summary>
/// Dashboard
/// </summary>
[Route("reports")]
[ApiController]
[ApiExplorerSettings(GroupName = "WMS")]
public class ReportsController(IReportService service) : BaseController
{
    private readonly IReportService _service = service;

    /// <summary>
    /// Get Inventories
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("inventories")]
    public async Task<ResultModel<IEnumerable<WarehouseInventoryReport>>> GetInventories([FromBody] InventoryReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.GetInventories(request, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<WarehouseInventoryReport>>.Error("data not found");
        }

        return ResultModel<IEnumerable<WarehouseInventoryReport>>.Success(data);        
    }

    /// <summary>
    /// Get Inventory Cards
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("inventory-cards")]
    public async Task<ResultModel<IEnumerable<InventoryCardItem>>> GetInventoryCards([FromBody] InventoryReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.GetInventoryCards(request, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<InventoryCardItem>>.Error("data not found");
        }

        return ResultModel<IEnumerable<InventoryCardItem>>.Success(data);
    }

    /// <summary>
    /// Search Stock On Shelf
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("search-stock-on-shelf")]
    public async Task<ResultModel<IEnumerable<StockOnShelfDto>>> SearchStockOnShelf([FromBody] InventoryReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.SearchStockOnShelf(request, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<StockOnShelfDto>>.Error("data not found");
        }

        return ResultModel<IEnumerable<StockOnShelfDto>>.Success(data);
    }
    /// <summary>
    /// Get Inventory In-Out Statements
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("inventory-in-out-statements")]
    public async Task<ResultModel<IEnumerable<InOutStatementDto>>> GetInventoryInOutStatements([FromBody] InventoryReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.GetInventoryInOutStatements(request, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<InOutStatementDto>>.Error("data not found");
        }

        return ResultModel<IEnumerable<InOutStatementDto>>.Success(data);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("low-stock-alerts")]
    public async Task<ResultModel<IEnumerable<LowStockAlertDto>>> GetLowStockAlerts(CancellationToken cancellationToken)
    {
        var data = await _service.GetLowStockAlerts(CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<LowStockAlertDto>>.Error("data not found");
        }

        return ResultModel<IEnumerable<LowStockAlertDto>>.Success(data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
        
    [HttpPost("outgoing-goods")]
    public async Task<ResultModel<IEnumerable<ExportReportItem>>> GetReportOutgoingGoods([FromBody] InventoryReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.GetReportOutgoingGoods(request, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<ExportReportItem>>.Error("data not found");
        }

        return ResultModel<IEnumerable<ExportReportItem>>.Success(data);
    }

    /// <summary>
    /// Report In-coming Goods
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("incoming-goods")]
    public async Task<ResultModel<IEnumerable<ImportReportItem>>> GetReportIncomingGoods([FromBody] InventoryReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.GetReportIncomingGoods(request, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<ImportReportItem>>.Error("data not found");
        }

        return ResultModel<IEnumerable<ImportReportItem>>.Success(data);
    }


    ///vendors
    [HttpGet("vendors")]
    public async Task<ResultModel<IEnumerable<VendorMaster>>> GetVendors(CancellationToken cancellationToken)
    {
        var data = await _service.GetVendors(CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<VendorMaster>>.Error("data not found");
        }

        return ResultModel<IEnumerable<VendorMaster>>.Success(data);
    }

    /// <summary>
    /// Ban Vendors
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("ban-vendors/{id}")]
    public async Task<ResultModel<int>> BanVendors(int id, CancellationToken cancellationToken)
    {
        var result = await _service.BanVendors(id, CurrentUser, cancellationToken);
        if (!result)
        {
            return ResultModel<int>.Error("Failed to Delete vendor");
        }

        return ResultModel<int>.Success(1);
    }
}
