namespace Wms.Theme.Web.Model.Warehouse;

public class InboundInfoModel: BaseInventoryOverview
{ 
    /// <summary>
    /// Id of Details
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// id of the receipt
    /// </summary>
    public int ReceiptId { get; set; }

    /// <summary>
    /// Receipt Number
    /// </summary>
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>
    /// Status of the receipt
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }

}


public class InventoryOverview : BaseInventoryOverview
{

}

public class OutboundInfoModel : BaseInventoryOverview
{
    /// <summary>
    /// Id of Details
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// id of the Order
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Order Number
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// Status of the Order
    /// </summary>
    public int Status { get; set; }
}

public class InventoryInfo
{
    public IEnumerable<InventoryOverview> Overview { get; set; } = [];
    public IEnumerable<InboundInfoModel> InboundInfo { get; set; } = [];
    public IEnumerable<OutboundInfoModel> OutboundInfo { get; set; } = [];
}

public class BaseInventoryOverview
{
    public int SkuId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string SkuName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int SkuUomId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    /// <summary>
    /// Availability (In Stock, Low Stock, Backordered...)
    /// </summary>
    public string InventoryStatus { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
}
