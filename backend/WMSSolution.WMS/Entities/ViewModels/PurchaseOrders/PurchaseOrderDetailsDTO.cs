using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WMSSolution.WMS.Entities.ViewModels.PurchaseOrders;

/// <summary>
/// 
/// </summary>
public class PurchaseOrderDetailsDTO
{
    /// <summary>
    /// id
    /// </summary>
    [JsonPropertyName("id")]
    public int id { get; set; } = 0;

    /// <summary>
    /// sku_id
    /// </summary>
    [JsonPropertyName("sku_id")]
    public int sku_id { get; set; } = 0;
    /// <summary>
    /// sku name
    /// </summary>
    [JsonPropertyName("sku_name")]
    public string sku_name { get; set; } = string.Empty;
    /// <summary>
    /// sku_code
    /// </summary>
    [JsonPropertyName("sku_code")]
    public string sku_code { get; set; } = string.Empty;
    /// <summary>
    /// spu id
    /// </summary>
    [JsonPropertyName("spu_id")]
    public int spu_id { get; set; } = 1;

    /// <summary>
    /// spu name
    /// </summary>
    [JsonPropertyName("spu_name")]
    public string spu_name { get; set; } = string.Empty;

    /// <summary>
    /// qty ordered
    /// </summary>
    [JsonPropertyName("qty_ordered")]
    public int qty_ordered { get; set; }

    /// <summary>
    /// qty received actually
    /// </summary>
    [JsonPropertyName("qty_received")]
    public int qty_received { get; set; } // SL Đã về

    /// <summary>
    /// the quantity still miss
    /// </summary>
    [NotMapped]
    public int qty_open => qty_ordered - qty_received;

    /// <summary>
    /// unit price
    /// </summary>
    [JsonPropertyName("unit_price")]
    public decimal unit_price { get; set; }

    /// <summary>
    /// exp of this item
    /// </summary>
    [JsonPropertyName("expiry_date")]
    public DateTime? expiry_date { get; set; } = DateTime.UtcNow;
}
