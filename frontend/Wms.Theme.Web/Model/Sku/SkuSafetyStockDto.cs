namespace Wms.Theme.Web.Model.Sku;

public class SkuSafetyStockDto
{
    public int Id { get; set; } = 0;
    public int? SkuId { get; set; }
    public string? SkuName { get; set; }
    public string? SkuCode { get; set; }
    public int? SafetyStockQty { get; set; }
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseAddress { get; set; }
}
