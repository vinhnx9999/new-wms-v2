namespace Wms.Theme.Web.Model.Sku;

public class SkuDTO
{
    /// <summary>
    /// sku id
    /// </summary>
    public int SkuId { get; set; } = 0;
    /// <summary>
    /// Sku name
    /// </summary>  
    public string SkuName { get; set; } = string.Empty;
    /// <summary>
    /// sku code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;
}

public class SkuSupplierDTO: SkuDTO
{    
    /// <summary>
    /// supplier id
    /// </summary>
    public int? SupplierId { get; set; } = 0;
    /// <summary>
    /// supplier name
    /// </summary>
    public string? SupplierName { get; set; } = string.Empty;
    /// <summary>
    /// sku uom id
    /// </summary>
    public int? SkuUomId { get; set; }
    /// <summary>
    ///  unit name
    /// </summary>
    public string? UnitName { get; set; }

    /// <summary>
    /// warehouse Id     
    /// </summary>
    public int? WarehouseId { get; set; }
    /// <summary>
    /// warehouse name
    /// </summary>
    public string? WarehouseName { get; set; }
    /// <summary>
    /// Location Id 
    /// </summary>
    public int? LocationId { get; set; }
    /// <summary>
    /// Location name 
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Total stock
    /// </summary>
    public decimal? TotalStock { get; set; }

    /// <summary>
    /// Available Qty = Total - Allocated
    /// </summary>
    public decimal? AvailableQty { get; set; }
}
