using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class WarehouseExcelImportViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public WarehouseExcelImportViewModel()
        {

        }
        #endregion
        #region Property


        /// <summary>
        /// WarehouseName
        /// </summary>
        [Display(Name = "WarehouseName")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        [Required(ErrorMessage = "Required")]
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// city
        /// </summary>
        [Display(Name = "city")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        [Required(ErrorMessage = "Required")]
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// address
        /// </summary>
        [Display(Name = "address")]
        [MaxLength(256, ErrorMessage = "MaxLength")]
        [Required(ErrorMessage = "Required")]
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
