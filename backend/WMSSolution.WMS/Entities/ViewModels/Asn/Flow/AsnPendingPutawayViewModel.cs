using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// pending putwaay viewModel
    /// </summary>
    public class AsnPendingPutawayViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public AsnPendingPutawayViewModel()
        {

        }
        #endregion

        #region Property

        /// <summary>
        /// asn_id
        /// </summary>
        [Display(Name = "asn_id")]
        public int asn_id { get; set; } = 0;

        /// <summary>
        /// goods_owner_id
        /// </summary>
        [Display(Name = "goods_owner_id")]
        public int goods_owner_id { get; set; } = 0;

        /// <summary>
        /// goods_owner_name
        /// </summary>
        [Display(Name = "goods_owner_name")]
        public string goods_owner_name { get; set; } = string.Empty;

        /// <summary>
        /// series_number
        /// </summary>
        [Display(Name = "series_number")]
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// sorted_qty
        /// </summary>
        [Display(Name = "sorted_qty")]
        public int sorted_qty { get; set; } = 0;

        /// <summary>
        /// asn_no
        /// </summary>
        [Display(Name = "asn_no")]
        public string asn_no { get; set; } = string.Empty;

        /// <summary>
        /// expiry_date
        /// </summary>
        [Display(Name = "expiry_date")]
        public DateTime expiry_date { get; set; } = DateTime.MinValue;

        /// <summary>
        /// sku_name
        /// </summary>
        [Display(Name = "sku_name")]
        public string sku_name { get; set; } = string.Empty;

        #endregion
    }
}
