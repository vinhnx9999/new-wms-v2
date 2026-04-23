using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Stock
{
    /// <summary>
    /// DTO for SKU available search response
    /// Used in POST /api/stock/sku-available
    /// </summary>
    public class SkuAvailableDTO
    {
        [JsonPropertyName("is_found")]
        public bool IsFound { get; set; }

        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; }

        [JsonPropertyName("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        [JsonPropertyName("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        [JsonPropertyName("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        [JsonPropertyName("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonPropertyName("qty_total")]
        public decimal QtyTotal { get; set; }

        [JsonPropertyName("qty_frozen")]
        public decimal QtyFrozen { get; set; }

        [JsonPropertyName("qty_locked_dispatch")]
        public decimal QtyLockedDispatch { get; set; }

        [JsonPropertyName("qty_locked_process")]
        public decimal QtyLockedProcess { get; set; }

        [JsonPropertyName("qty_locked_move")]
        public decimal QtyLockedMove { get; set; }

        [JsonPropertyName("qty_locked")]
        public decimal QtyLocked { get; set; }

        [JsonPropertyName("qty_available")]
        public decimal QtyAvailable { get; set; }
    }
}
