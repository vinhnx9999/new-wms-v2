using Mapster;

namespace WMSSolution.WMS.Entities.ViewModels.Sku
{
    /// <summary>
    /// Update Sku Request
    /// </summary>
    public class UpdateSkuRequest
    {
        /// <summary>
        /// Sku Name
        /// </summary>
        [AdaptMember("sku_name")]
        public string? SkuName { get; set; }
        /// <summary>
        /// Sku Code
        /// </summary>
        [AdaptMember("sku_code")]
        public string? SkuCode { get; set; }
        /// <summary>
        /// Bar code 
        /// </summary>
        [AdaptMember("bar_code")]
        public string? BarCode { get; set; }
        /// <summary>
        /// Max qty per pallet
        /// </summary>
        [AdaptMember("maxQtyPerPallet")]
        public int? MaxQtyPerPallet { get; set; }
        /// <summary>
        /// SkuOumId 
        /// </summary>
        public int? SkuUomId { get; set; }

    }
}
