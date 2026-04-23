namespace WMSSolution.WMS.Entities.ViewModels.Receipt;

/// <summary>
/// Base Receipt Dto
/// </summary>
public class BaseReceiptDto
{
    /// <summary>
    /// Receipt Number
    /// </summary>
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Priority
    /// default value with the min
    /// </summary>
    public int Priority { get; set; } = 1;

}

/// <summary>
/// Base Receipt Detail Dto
/// </summary>
public class BaseReceiptDetailDto
{
    /// <summary>
    /// Id of detail item
    /// </summary>

    public int? Id { get; set; } = 0;
    /// <summary>
    /// SKU ID
    /// </summary>
    public int SkuId { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of Measure ID
    /// </summary>
    public int SkuUomId { get; set; }

    /// <summary>
    /// Goods Location ID (Stock Location)
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// Source number
    /// </summary>
    public string? SourceNumber { get; set; }
}