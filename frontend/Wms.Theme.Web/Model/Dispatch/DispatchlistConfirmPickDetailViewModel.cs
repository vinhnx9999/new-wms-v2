using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    public class DispatchlistConfirmPickDetailViewModel
    {
        /// <summary>
        /// stock_id
        /// </summary>
        [JsonPropertyName("stock_id")]
        public int StockId { get; set; } = 0;

        /// <summary>
        /// dispatchlist_id
        /// </summary>
        [JsonPropertyName("dispatchlist_id")]
        public int DispatchlistId { get; set; } = 0;

        /// <summary>
        /// goods_owner_id
        /// </summary>
        [JsonPropertyName("goods_owner_id")]
        public int GoodsOwnerId { get; set; } = 0;

        /// <summary>
        /// goods_owner_name
        /// </summary>
        [JsonPropertyName("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;

        /// <summary>
        /// goods_location_id
        /// </summary>
        [JsonPropertyName("goods_location_id")]
        public int GoodsLocationId { get; set; } = 0;

        /// <summary>
        /// warehouse_name
        /// </summary>
        [JsonPropertyName("warehouse_name")]
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// location_name
        /// </summary>
        [JsonPropertyName("location_name")]
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// warehouse_area_name
        /// </summary>
        [JsonPropertyName("warehouse_area_name")]
        public string WarehouseAreaName { get; set; } = string.Empty;

        /// <summary>
        /// quantity available
        /// </summary>
        [JsonPropertyName("qty_available")]
        public int QtyAvailable { get; set; } = 0;

        /// <summary>
        /// pick_qty
        /// </summary>
        [JsonPropertyName("pick_qty")]
        public int PickQty { get; set; } = 0;

        /// <summary>
        /// series_number
        /// </summary>
        [JsonPropertyName("series_number")]
        public string SeriesNumber { get; set; } = string.Empty;

        /// <summary>
        /// expiry_date
        /// </summary>
        [JsonPropertyName("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// price
        /// </summary>
        [JsonPropertyName("price")]
        public decimal Price { get; set; } = 0;

        /// <summary>
        /// putaway_date
        /// </summary>
        [JsonPropertyName("putaway_date")]
        public DateTime PutawayDate { get; set; }
    }
}