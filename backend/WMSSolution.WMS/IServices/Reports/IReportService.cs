
using WMSSolution.Core.JWT;
using WMSSolution.Core.Services;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.Models.Stock;
using WMSSolution.WMS.Entities.ViewModels.Reports;

namespace WMSSolution.WMS.IServices.Reports;

/// <summary>
/// Stock Reports Service
/// </summary>
public interface IReportService : IBaseService<StockTransactionEntity>
{
    /// <summary>
    /// Ban-Vendors
    /// </summary>
    /// <param name="vendorId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> BanVendors(int vendorId, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Get Inventories
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<WarehouseInventoryReport>> GetInventories(InventoryReportRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get inventory cards
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<InventoryCardItem>> GetInventoryCards(InventoryReportRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Inventory In-Out Statements
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<InOutStatementDto>> GetInventoryInOutStatements(InventoryReportRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Get Low Stock Alerts
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlerts(CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Report Incoming Goods
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<ImportReportItem>> GetReportIncomingGoods(InventoryReportRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Report Outgoing Goods
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<ExportReportItem>> GetReportOutgoingGoods(InventoryReportRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Vendors
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<VendorMaster>> GetVendors(CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Search Stock On Shelf
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<StockOnShelfDto>> SearchStockOnShelf(InventoryReportRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
}
