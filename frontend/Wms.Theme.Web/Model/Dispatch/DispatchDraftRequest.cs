using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    /// <summary>
    /// Request model for creating draft dispatch order
    /// Used in POST /api/dispatchlist/draft and POST /api/dispatchlist/create-and-execute
    /// </summary>
    public class DispatchDraftRequest
    {
        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("contact")]
        public string Contact { get; set; } = string.Empty;

        [JsonPropertyName("contact_tel")]
        public string ContactTel { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<DispatchDraftItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Item in dispatch draft request
    /// </summary>
    public class DispatchDraftItem
    {
        [JsonPropertyName("sku_id")]
        public int SkuId { get; set; }

        [JsonPropertyName("qty")]
        public decimal Qty { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("pallet_selections")]
        public List<PalletSelection> PalletSelections { get; set; } = new();
    }

    /// <summary>
    /// Pallet selection for dispatch
    /// </summary>
    public class PalletSelection
    {
        [JsonPropertyName("pallet_id")]
        public int PalletId { get; set; }

        [JsonPropertyName("pallet_code")]
        public string PalletCode { get; set; } = string.Empty;

        [JsonPropertyName("location_id")]
        public int LocationId { get; set; }

        [JsonPropertyName("goods_owner_id")]
        public int GoodsOwnerId { get; set; }

        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("putaway_date")]
        public DateTime? PutawayDate { get; set; }

        [JsonPropertyName("pick_qty")]
        public decimal PickQty { get; set; }
    }
}
