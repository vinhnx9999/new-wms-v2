using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Stock;

/// <summary>
/// DTO for stock location with available quantity for outbound picking
/// </summary>
public class StockLocationDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sku_id")]
    public int SkuId { get; set; }

    [JsonPropertyName("sku_code")]
    public string SkuCode { get; set; } = string.Empty;

    [JsonPropertyName("sku_name")]
    public string SkuName { get; set; } = string.Empty;

    [JsonPropertyName("goods_location_id")]
    public int GoodsLocationId { get; set; }

    [JsonPropertyName("location_name")]
    public string LocationName { get; set; } = string.Empty;

    [JsonPropertyName("warehouse_id")]
    public int WarehouseId { get; set; }

    [JsonPropertyName("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    [JsonPropertyName("qty_available")]
    public decimal QtyAvailable { get; set; }

    [JsonPropertyName("qty_locked")]
    public decimal QtyLocked { get; set; }

    [JsonPropertyName("batch_number")]
    public string? BatchNumber { get; set; }

    [JsonPropertyName("pallet_id")]
    public int? PalletId { get; set; }

    [JsonPropertyName("pallet_code")]
    public string? PalletCode { get; set; }

    [JsonPropertyName("expiry_date")]
    public DateTime? ExpiryDate { get; set; }

    [JsonPropertyName("production_date")]
    public DateTime? ProductionDate { get; set; }
}
