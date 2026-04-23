using Wms.Theme.Web.Model.Reports;
using WMSSolution.Shared.MasterData;

namespace Wms.Theme.Web.Services.Reports;

public interface IReportService
{
    Task<(int? data, string? message)> BanVendor(long vendorId);
    Task<IEnumerable<InOutStatementDto>> GetInOutStatements(InventoryReportRequest request);
    Task<IEnumerable<WarehouseInventoryReport>> GetInventories(InventoryReportRequest request);
    Task<IEnumerable<InventoryCardItem>> GetInventoryCards(InventoryReportRequest request);
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlerts();
    Task<IEnumerable<ImportReportItem>> GetReportIncomingGoods(InventoryReportRequest request);
    Task<IEnumerable<ExportReportItem>> GetReportOutgoingGoods(InventoryReportRequest request);
    Task<IEnumerable<VendorMaster>> GetVendors();
    Task<IEnumerable<StockOnShelfDto>> SearchStockOnShelf(InventoryReportRequest request);
}
