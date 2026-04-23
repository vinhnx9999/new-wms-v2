

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// get stock infomation by phone api input viewmodel
    /// </summary>
    public class LocationStockForPhoneSearchViewModel
    {
        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// goods_location_id
        /// </summary>
        public int goods_location_id { get; set; } = 0;

        /// <summary>
        /// WarehouseId
        /// </summary>
        public int WarehouseId { get; set; } = 0;

        /// <summary>
        /// spu name
        /// </summary>
        public string spu_name { get; set; } = string.Empty;

        /// <summary>
        /// location name
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime expiry_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;
    }
}