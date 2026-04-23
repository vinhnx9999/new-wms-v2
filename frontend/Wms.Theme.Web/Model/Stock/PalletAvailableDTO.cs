using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Stock
{
    /// <summary>
    /// DTO for available pallet/location for outbound
    /// Used in GET /api/stock/available-for-outbound
    /// </summary>
    public class PalletAvailableDTO
    {
        [JsonPropertyName("pallet_id")]
        public int PalletId { get; set; }

        [JsonPropertyName("pallet_code")]
        public string PalletCode { get; set; } = string.Empty;

        [JsonPropertyName("location_id")]
        public int LocationId { get; set; }

        [JsonPropertyName("location_name")]
        public string LocationName { get; set; } = string.Empty;

        [JsonPropertyName("warehouse_name")]
        public string WarehouseName { get; set; } = string.Empty;

        [JsonPropertyName("goods_owner_id")]
        public int GoodsOwnerId { get; set; }

        [JsonPropertyName("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;

        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("putaway_date")]
        public DateTime? PutawayDate { get; set; }

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

        [JsonPropertyName("suggested_qty")]
        public decimal SuggestedQty { get; set; }

        [JsonPropertyName("selected_qty")]
        public decimal SelectedQty { get; set; }
    }
}
