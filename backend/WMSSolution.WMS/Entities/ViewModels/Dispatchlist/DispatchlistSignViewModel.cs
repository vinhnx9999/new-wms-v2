using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// DispatchlistSignViewModel
    /// </summary>
    public class DispatchlistSignViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public DispatchlistSignViewModel()
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
        /// damage_qty
        /// </summary>
        public int damage_qty { get; set; } = 0;

        #endregion
    }
}
