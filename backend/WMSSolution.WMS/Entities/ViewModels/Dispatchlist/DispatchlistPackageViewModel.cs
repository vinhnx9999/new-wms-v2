using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// DispatchlistPackageViewModel
    /// </summary>
    public class DispatchlistPackageViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public DispatchlistPackageViewModel()
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
        /// package_qty
        /// </summary>
        [Display(Name = "package_qty")]
        public int package_qty { get; set; } = 0;

        /// <summary>
        /// picked_qty
        /// </summary>
        [Display(Name = "picked_qty")]
        public int picked_qty { get; set; } = 0;
        #endregion
    }
}
