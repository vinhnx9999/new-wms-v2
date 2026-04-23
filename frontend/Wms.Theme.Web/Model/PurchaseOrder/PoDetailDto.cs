namespace Wms.Theme.Web.Model.PurchaseOrder;

public class PoDetailDto
{
    public int Id { get; set; }
    public string PoNo { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public int PoStatus { get; set; }

    public string? BuyerName { get; set; }
    public string? BuyerAddress { get; set; }
    public string? PaymentTerm { get; set; }
    public string? Description { get; set; }
    public decimal? ShippingAmount { get; set; }
    public decimal? TotalAmount { get; set; }

    public List<PoDetailItemDto> Details { get; set; } = [];
}

public class PoDetailItemDto
{
    public int Id { get; set; }
    public int SkuId { get; set; }
    public string? SkuCode { get; set; }
    public string? SkuName { get; set; }

    public decimal QtyOrdered { get; set; }
    public decimal QtyReceived { get; set; }
    public decimal? UnitPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }

    public int SkuUomId { get; set; }
    public string? UnitName { get; set; }
}