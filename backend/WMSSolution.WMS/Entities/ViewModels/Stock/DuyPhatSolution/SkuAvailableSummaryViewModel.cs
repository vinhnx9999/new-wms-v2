using Newtonsoft.Json;

namespace WMSSolution.WMS.Entities.ViewModels.Stock.DuyPhatSolution
{
    /// <summary>
    /// Sku available summary view model
    /// </summary>

    public class SkuAvailableSummaryViewModel
    {
        /// <summary>
        /// flag to chẹck
        /// </summary>
        [JsonProperty("is_found")]
        public bool IsFound { get; set; }

        /// <summary>
        /// sku id
        /// </summary>
        [JsonProperty("sku_id")]
        public int SkuId { get; set; }

        /// <summary>
        /// sku code
        /// </summary>
        [JsonProperty("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        /// <summary>
        /// sku name
        /// </summary>
        [JsonProperty("sku_name")]
        public string SkuName { get; set; } = string.Empty;

        /// <summary>
        /// spu code
        /// </summary>
        [JsonProperty("spu_code")]
        public string SpuCode { get; set; } = string.Empty;

        /// <summary>
        /// Spu name 
        /// </summary>
        [JsonProperty("spu_name")]
        public string SpuName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unit")]
        public string Unit { get; set; } = string.Empty;


        /// <summary>
        /// Total quantity in warehouse
        /// </summary>
        [JsonProperty("qty_total")]
        public decimal QtyTotal { get; set; }

        /// <summary>
        /// Frozen quantity
        /// </summary>
        [JsonProperty("qty_frozen")]
        public decimal QtyFrozen { get; set; }

        /// <summary>
        /// Locked by outbound orders
        /// </summary>
        [JsonProperty("qty_locked_dispatch")]
        public decimal QtyLockedDispatch { get; set; }

        /// <summary>
        /// Locked by internal transfer
        /// </summary>
        [JsonProperty("qty_locked_process")]
        public decimal QtyLockedProcess { get; set; }

        /// <summary>
        /// Locked by stock move
        /// </summary>
        [JsonProperty("qty_locked_move")]
        public decimal QtyLockedMove { get; set; }

        /// <summary>
        /// Total locked quantity
        /// </summary>
        [JsonProperty("qty_locked")]
        public decimal QtyLocked { get; set; }

        /// <summary>
        /// Available quantity for new outbound
        /// </summary>
        [JsonProperty("qty_available")]
        public decimal QtyAvailable { get; set; }
    }
}
