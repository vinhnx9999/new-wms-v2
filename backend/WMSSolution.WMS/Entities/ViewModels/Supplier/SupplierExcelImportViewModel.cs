using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class SupplierExcelImportViewModel
    {

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public SupplierExcelImportViewModel()
        {

        }
        #endregion
        #region Property

        /// <summary>
        /// supplier_name
        /// </summary>
        [Display(Name = "supplier_name")]
        [MaxLength(256, ErrorMessage = "MaxLength")]
        [Required(ErrorMessage = "Required")]
        public string supplier_name { get; set; } = string.Empty;

        /// <summary>
        /// city
        /// </summary>
        [Display(Name = "city")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// address
        /// </summary>
        [Display(Name = "address")]
        [MaxLength(256, ErrorMessage = "MaxLength")]
        public string address { get; set; } = string.Empty;

        /// <summary>
        /// email
        /// </summary>
        [Display(Name = "email")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// manager
        /// </summary>
        [Display(Name = "manager")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string manager { get; set; } = string.Empty;

        /// <summary>
        /// contact_tel
        /// </summary>
        [Display(Name = "contact_tel")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string contact_tel { get; set; } = string.Empty;
        #endregion
    }
}
