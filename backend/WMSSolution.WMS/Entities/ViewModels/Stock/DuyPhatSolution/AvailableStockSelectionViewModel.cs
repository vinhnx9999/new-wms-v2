using Newtonsoft.Json;

namespace WMSSolution.WMS.Entities.ViewModels.Stock.DuyPhatSolution
{
    /// <summary>
    /// available stock selection view model
    /// </summary>
    public class AvailableStockSelectionViewModel
    {
        /// <summary>
        /// pallet id
        /// </summary>
        [JsonProperty("pallet_id")]
        public int PalletId { get; set; }

        /// <summary>
        /// pallet code
        /// </summary>
        [JsonProperty("pallet_code")]
        public string PalletCode { get; set; } = string.Empty;

        /// <summary>
        /// location id
        /// </summary>
        [JsonProperty("location_id")]
        public int LocationId { get; set; }

        /// <summary>
        /// location name
        /// </summary>
        [JsonProperty("location_name")]
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// warehouse name
        /// </summary>
        [JsonProperty("warehouse_name")]
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// good owner id
        /// </summary>
        [JsonProperty("goods_owner_id")]
        public int GoodsOwnerId { get; set; }

        /// <summary>
        /// good owner name
        /// </summary>
        [JsonProperty("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;

        /// <summary>
        /// expiry date
        /// </summary>
        [JsonProperty("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// putaway date
        /// </summary>
        [JsonProperty("putaway_date")]
        public DateTime PutAwayDate { get; set; }

        /// <summary>
        /// qty total
        /// </summary>
        [JsonProperty("qty_total")]
        public decimal QtyTotal { get; set; }

        /// <summary>
        /// qty frozen
        /// </summary>
        [JsonProperty("qty_frozen")]
        public decimal QtyFrozen { get; set; }

        /// <summary>
        /// qty locked dispatch
        /// </summary>
        [JsonProperty("qty_locked_dispatch")]
        public decimal QtyLockedDispatch { get; set; }

        /// <summary>
        /// qty locked process
        /// </summary>
        [JsonProperty("qty_locked_process")]
        public decimal QtyLockedProcess { get; set; }

        /// <summary>
        /// qty locked move
        /// </summary>
        [JsonProperty("qty_locked_move")]
        public decimal QtyLockedMove { get; set; }

        /// <summary>
        /// qty locked
        /// </summary>
        [JsonProperty("qty_locked")]
        public decimal QtyLocked { get; set; }

        /// <summary>
        /// qty available
        /// </summary>
        [JsonProperty("qty_available")]
        public decimal QtyAvailable { get; set; }

        /// <summary>
        /// suggested qty
        /// </summary>
        [JsonProperty("suggested_qty")]
        public decimal SuggestedQty { get; set; }

        /// <summary>
        /// selected qty
        /// </summary>
        [JsonProperty("selected_qty")]
        public decimal SelectedQty { get; set; }
    }
}
