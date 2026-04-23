using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// DispatchlistWeightViewModel
    /// </summary>
    public class DispatchlistWeightViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public DispatchlistWeightViewModel()
        {

        }
        #endregion
        #region Property
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "id")]
        public int id { get; set; } = 0;
        /// <summary>
        /// dispatch_no
        /// </summary>
        [Display(Name = "dispatch_no")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string dispatch_no { get; set; } = string.Empty;

        /// <summary>
        /// dispatch_status
        /// </summary>
        [Display(Name = "dispatch_status")]
        public byte dispatch_status { get; set; } = 0;

        /// <summary>
        /// weighing_qty
        /// </summary>
        [Display(Name = "weighing_qty")]
        public int weighing_qty { get; set; } = 0;

        /// <summary>
        /// weighing_weight
        /// </summary>
        [Display(Name = "weighing_weight")]
        public decimal weighing_weight { get; set; } = 0;

        /// <summary>
        /// picked_qty
        /// </summary>
        [Display(Name = "picked_qty")]
        public int picked_qty { get; set; } = 0;
        #endregion
    }
}
