
using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// customer view model
    /// </summary>
    public class CustomerViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public CustomerViewModel()
        {

        }
        #endregion

        #region Property

        /// <summary>
        /// primary key
        /// </summary>
        public int id { get; set; } = 0;

        /// <summary>
        /// customer's name
        /// </summary>
        [Display(Name = "customer_name")]
        [Required(ErrorMessage = "Required")]
        [MaxLength(256, ErrorMessage = "MaxLength")]
        public string customer_name { get; set; } = string.Empty;

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
        /// manager
        /// </summary>
        [Display(Name = "manager")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string manager { get; set; } = string.Empty;

        /// <summary>
        /// email
        /// </summary>
        [Display(Name = "email")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// contact tel
        /// </summary>
        [Display(Name = "contact_tel")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string contact_tel { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        [Display(Name = "creator")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create time
        /// </summary>
        [Display(Name = "create_time")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last update time
        /// </summary>
        [Display(Name = "last_update_time")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// valid
        /// </summary>
        [Display(Name = "is_valid")]
        public bool is_valid { get; set; } = true;
        /// <summary>
        /// TaxNumber
        /// </summary>
        public string tax_number { get; set; } = string.Empty;
        #endregion
    }

    /// <summary>
    /// Customer response list item view model
    /// </summary>
    public class CustomerResponseViewModel
    {
        /// <summary>
        /// primary key
        /// </summary>
        public int ID { get; set; } = 0;

        /// <summary>
        /// customer's name
        /// </summary>

        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// city
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// address
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// manager
        /// </summary>
        public string Manager { get; set; } = string.Empty;

        /// <summary>
        /// email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// contact tel
        /// </summary>
        public string ContactTel { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// create time
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last update time
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// valid
        /// </summary>
        public bool IsValid { get; set; } = true;
    }
}
