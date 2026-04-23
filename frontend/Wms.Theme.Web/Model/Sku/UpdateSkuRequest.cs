namespace Wms.Theme.Web.Model.Sku;

public class UpdateSkuRequest
{
    /// <summary>
    /// Sku Name
    /// </summary>
    public string? SkuName { get; set; }
    /// <summary>
    /// Sku Code
    /// </summary>
    public string? SkuCode { get; set; }
    /// <summary>
    /// Bar code 
    /// </summary>

    public string? BarCode { get; set; }
    /// <summary>
    /// Max qty per pallet
    /// </summary>
    /// 
    public int? MaxQtyPerPallet { get; set; }

    /// <summary>
    /// SkuOumId 
    /// </summary>
    public int? SkuUomId { get; set; }
    /// <summary>
    /// Unit Name 
    /// </summary>
}
