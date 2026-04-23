namespace WMSSolution.WMS.Entities.ViewModels.Warehouse.Invertory;

/// <summary>
/// InboundInfo Model
/// </summary>
public class InboundInfoModel : BaseInventoryOverview
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
    /// <summary>
    /// Created Date
    /// </summary>
    public DateTime? CreateDate { get; set; }
}

/// <summary>
/// Represents an overview of the inventory, providing essential details about the current stock levels and item
/// statuses.
/// </summary>
/// <remarks>This class inherits from BaseInventoryOverview, which may provide additional functionality or
/// properties relevant to inventory management.</remarks>
public class InventoryOverview : BaseInventoryOverview
{
    public int OrderId { get; internal set; }
}

/// <summary>
/// Outbound Info Model
/// </summary>
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
    /// <summary>
    /// Customer Id
    /// </summary>
    public int CustomerId { get; set; }
    /// <summary>
    /// Customer Name
    /// </summary>
    public string? CustomerName { get; set; } 
    /// <summary>
    /// Create Date
    /// </summary>
    public DateTime? CreateDate { get; set; }
    /// <summary>
    /// Type
    /// </summary>
    public string? Type { get; set; }
    /// <summary>
    /// Gets or sets the name of the gateway used for communication.
    /// </summary>
    public string? GatewayName { get; set; }
}

/// <summary>
/// Inventory Info
/// </summary>
public class InventoryInfo
{
    /// <summary>
    /// Overview
    /// </summary>
    public IEnumerable<InventoryOverview> Overview { get; set; } = [];
    /// <summary>
    /// Inbound Info
    /// </summary>
    public IEnumerable<InboundInfoModel> InboundInfo { get; set; } = [];
    /// <summary>
    /// Outbound Info
    /// </summary>
    public IEnumerable<OutboundInfoModel> OutboundInfo { get; set; } = [];
}

/// <summary>
/// Base Inventory Overview
/// </summary>
public class BaseInventoryOverview
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; }
    /// <summary>
    /// Sku Code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;
    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;
    /// <summary>
    /// Category
    /// </summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the stock keeping unit (SKU) unit of measure.
    /// </summary>
    /// <remarks>This property is used to associate a specific unit of measure with the SKU, which can be
    /// important for inventory management and sales processing.</remarks>
    public int SkuUomId { get; set; }
    /// <summary>
    /// Gets or sets the name of the unit associated with the measurement.
    /// </summary>
    /// <remarks>This property is typically used to specify the unit of measurement for a value, such as
    /// 'meters' or 'kilograms'.</remarks>
    public string UnitName { get; set; } = string.Empty;
    /// <summary>
    /// Unit Price
    /// </summary>
    public decimal? UnitPrice { get; set; }
    /// <summary>
    /// Location Id
    /// </summary>
    public int? LocationId { get; set; }
    /// <summary>
    /// Gets or sets the name of the location associated with the entity.
    /// </summary>
    /// <remarks>This property can be null if the location name is not specified. It is recommended to provide
    /// a meaningful name to enhance clarity in context.</remarks>
    public string? LocationName { get; set; }
    /// <summary>
    /// Supplier Id
    /// </summary>
    public int SupplierId { get; set; }
    /// <summary>
    /// Supplier Name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;
    /// <summary>
    /// Availability (In Stock, Low Stock, Backordered...)
    /// </summary>
    public string InventoryStatus { get; set; } = string.Empty;
    /// <summary>
    /// Expiry Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
}