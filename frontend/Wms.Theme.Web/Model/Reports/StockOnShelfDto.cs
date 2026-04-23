using Wms.Theme.Web.Util;
namespace Wms.Theme.Web.Model.Reports;

/// <summary>
/// Exact storage location in the warehouse
/// </summary>
public class StockOnShelfDto
{
    /**
     Area / Rack / Shelf: Exact storage location in the warehouse.
    Product Code / Product Name: Identification details of the item.
    Batch No. / Expiry Date: Batch tracking, crucial for food and pharmaceuticals.
    UOM (Unit of Measure): Case, box, bottle, etc.
    Stock Qty: Total quantity physically present.
    Available Qty: Quantity ready for sale/dispatch (excluding reserved or QC items).
    Notes: Special conditions (pending dispatch, reserved, under quality check).
     */

    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int? LocationId { get; set; }
    public string LocationName { get; set; } = "";
    public int SkuId { get; set; }
    public int UnitId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal? Quantity { get; set; } = 0;
    public decimal? AvailableQty { get; set; } = 0;
    /// <summary>
    /// Ngày phát sinh nghiệp vụ
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; } = "";
    public int? PalletId { get; set; }
    public string? PalletName { get; set; } = "";
    public string ExpiryDateStr
    {
        get
        {
            if (!ExpiryDate.HasValue || ExpiryDate.GetValueOrDefault().Year < 2000) return "";
            return ExpiryDate.GetValueOrDefault().Convert2LocalDate();
        }
    }
}
