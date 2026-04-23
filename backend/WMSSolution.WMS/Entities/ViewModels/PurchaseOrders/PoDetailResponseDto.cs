namespace WMSSolution.WMS.Entities.ViewModels.PurchaseOrders;

/// <summary>
/// Po Detail
/// </summary>
public class PoDetailResponseDto
{
    /// <summary>
    /// Id 
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Po no
    /// </summary>
    public string PoNo { get; set; } = default!;

    /// <summary>
    /// SupplierId
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// SupplierName
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// Order Date
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Expected Delivery Date
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// Po status
    /// </summary>
    public int PoStatus { get; set; } = default!;

    /// <summary>
    /// Buyer name
    /// </summary>
    public string? BuyerName { get; set; }

    /// <summary>
    /// Buyer address
    /// </summary>
    public string? BuyerAddress { get; set; }

    /// <summary>
    /// Payment term
    /// </summary>
    public string? PaymentTerm { get; set; }

    /// <summary>
    /// Desceiption
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Shipping amount 
    /// </summary>
    public decimal? ShippingAmount { get; set; }

    /// <summary>
    /// Total Amount
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Details
    /// </summary>
    public List<PoDetailItemDto> Details { get; set; } = [];
}

/// <summary>
/// Item
/// </summary>
public class PoDetailItemDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Skuid
    /// </summary>
    public int SkuId { get; set; }

    /// <summary>
    /// Sku code
    /// </summary>
    public string? SkuCode { get; set; }

    /// <summary>
    /// Skuname
    /// </summary>
    public string? SkuName { get; set; }

    /// <summary>
    /// qty orrdered    
    /// </summary>

    public decimal QtyOrdered { get; set; }

    /// <summary>
    /// qty received
    /// </summary>
    public decimal QtyReceived { get; set; }

    /// <summary>
    /// Unit price
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Exp 
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Supplier Id
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Supplier name
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// SkuUomId 
    /// </summary>
    public int SkuUomId { get; set; }

    /// <summary>
    /// Unit name
    /// </summary>
    public string? UnitName { get; set; }
}