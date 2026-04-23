using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.PurchaseOrder;

public class PageSearchPOResponse
{
    public int Id { get; set; }
    public string PoNo { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public int PoStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string Creator { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}

public class CreateNewOrderRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("po_no")]
    public string PoNo { get; set; } = string.Empty;
    [JsonPropertyName("supplier_id")]
    public int SupplierId { get; set; }
    [JsonPropertyName("supplier_name")]
    public string SupplierName { get; set; } = string.Empty;
    [JsonPropertyName("order_date")]
    public DateTime OrderDate { get; set; }
    [JsonPropertyName("expected_delivery_date")]
    public DateTime ExpectedDeliveryDate { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("po_status")]
    public int PoStatus { get; set; }
    [JsonPropertyName("creator")]
    public string Creator { get; set; } = string.Empty;
    [JsonPropertyName("details")]
    public List<PurchaseOrderDetailsDTO> Details { get; set; } = [];
}

public class PurchaseOrderDetailsDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("sku_id")]
    public int SkuId { get; set; }
    [JsonPropertyName("sku_code")]
    public string SkuCode { get; set; } = string.Empty;
    [JsonPropertyName("sku_name")]
    public string SkuName { get; set; } = string.Empty;
    [JsonPropertyName("spu_id")]
    public int SpuId { get; set; }
    [JsonPropertyName("spu_name")]
    public string SpuName { get; set; } = string.Empty;
    [JsonPropertyName("qty_ordered")]
    public int QtyOrdered { get; set; }
    [JsonPropertyName("qty_received")]
    public int QtyReceived { get; set; }
    [JsonPropertyName("qty_open")]
    public int QtyOpen { get; set; }
    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }
    [JsonPropertyName("expiry_date")]
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow;
}

public class PurchaseOrderResponse
{
    public List<PageSearchPOResponse> Rows { get; set; } = [];
    public int Totals { get; set; } = 0;
}

