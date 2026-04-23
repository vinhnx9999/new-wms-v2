using Wms.Theme.Web.Util;
namespace Wms.Theme.Web.Model.Reports;
public class NormalReportItem
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;
    public int? ReceiptId { get; set; }
    public string SerialNumber { get; set; } = "";
    public DateTime? ReceiptDate { get; set; }

    public int SkuId { get; set; }
    public int? UnitId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 0;
    
    public string? Description { get; set; } = "";
   
    public string ReceiptDateStr
    {
        get
        {
            if (!ReceiptDate.HasValue || ReceiptDate.GetValueOrDefault().Year < 2000) return "";
            return ReceiptDate.GetValueOrDefault().Convert2LocalDate();
        }
    }
}

public class ExportReportItem : NormalReportItem
{
    public decimal OutcomingQty { get; set; } = 0;
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime? ExportDate { get; set; }
    public string ExportDateStr
    {
        get
        {
            if (!ExportDate.HasValue || ExportDate.GetValueOrDefault().Year < 2000) return "";
            return ExportDate.GetValueOrDefault().Convert2LocalDate();
        }
    }
}


public class ImportReportItem : NormalReportItem
{
    public decimal IncomingQty { get; set; } = 0;
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? ImportDate { get; set; }
    public string ImportDateStr
    {
        get
        {
            if (!ImportDate.HasValue || ImportDate.GetValueOrDefault().Year < 2000) return "";
            return ImportDate.GetValueOrDefault().Convert2LocalDate();
        }
    }
}
