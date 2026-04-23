namespace Wms.Theme.Web.Model.Reports;

public class InventoryReportRequest
{
    public int? WarehouseId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public IEnumerable<int> SkuIds { get; set; } = [];
}
