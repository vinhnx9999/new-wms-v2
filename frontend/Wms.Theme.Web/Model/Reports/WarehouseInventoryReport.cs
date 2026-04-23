namespace Wms.Theme.Web.Model.Reports;

public class WarehouseInventoryReport
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;

    public List<InventoryReportItem> Items { get; set; } = [];

    public decimal TotalQuantityReceived 
    { 
        get
        {
            return Items.Sum(x => x.InwardQuantity);
        }           
    }

    public decimal TotalQuantityIssued
    {
        get
        {
            return Items.Sum(x => x.OutwardQuantity);
        }
    }

    public decimal TotalQuantityClosing
    {
        get
        {
            return Items.Sum(x => x.ClosedBalance);
        }
    }
}
