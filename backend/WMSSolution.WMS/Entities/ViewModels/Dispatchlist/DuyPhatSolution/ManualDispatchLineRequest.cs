using Newtonsoft.Json;

namespace WMSSolution.WMS.Entities.ViewModels.Dispatchlist.Duy_Phat_Solution
{
    /// <summary>
    /// Dispatch item request - represents a single SKU line in the dispatch order
    /// </summary>
    public class ManualDispatchLineRequest
    {
        /// <summary>
        /// SKU ID (Stock Keeping Unit identifier)
        /// </summary>
        [JsonProperty("sku_id")]
        public int SkuId { get; set; }

        /// <summary>
        /// Unit of Measurement ID (e.g., pieces, boxes, pallets)
        /// </summary>
        [JsonProperty("uom_id")]
        public int UomId { get; set; }

        /// <summary>
        /// Quantity to dispatch (supports decimal for mixed warehouse)
        /// </summary>
        [JsonProperty("req_qty")]
        public decimal ReqQty { get; set; }

        /// <summary>
        /// Additional description or notes for this item
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }
        /// <summary>
        /// selected
        /// </summary>
        public List<ManualPickLocationRequest> SelectedLocations { get; set; } = new List<ManualPickLocationRequest>();
    }

    /// <summary>
    /// represent pick location for manual dispatch line
    /// </summary>
    public class ManualPickLocationRequest
    {
        /// <summary>
        /// pallet id
        /// </summary>
        [JsonProperty("pallet_id")]
        public int PalletId { get; set; }

        /// <summary>
        /// location id
        /// </summary>
        [JsonProperty("location_id")]
        public int LocationId { get; set; }

        /// <summary>
        /// pick qty in this location
        /// </summary>

        public decimal PickQty { get; set; }
    }
}
