using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{

    /// <summary>
    /// Create User ViewModel
    /// </summary>
    public class CreateUserViewModel
    {
        /// <summary>
        /// UserName
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string UserName { get; set; } = "";
        /// <summary>
        /// Display Name
        /// </summary>
        public string DisplayName { get; set; } = "";
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = "";
        /// <summary>
        /// Cellphone
        /// </summary>
        public string ContactTel { get; set; } = "";
    }
    /// <summary>
    /// 
    /// </summary>
    public class UserViewModel
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public UserViewModel()
        {
        }

        #endregion constructor

        #region property

        /// <summary>
        /// primary key
        /// </summary>
        public int id { get; set; } = 0;

        /// <summary>
        /// user's number
        /// </summary>
        [Display(Name = "user_num")]
        [Required(ErrorMessage = "Required")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string user_num { get; set; } = string.Empty;

        /// <summary>
        /// user's name
        /// </summary>
        [Display(Name = "user_name")]
        [Required(ErrorMessage = "Required")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string user_name { get; set; } = string.Empty;

        /// <summary>
        /// contact
        /// </summary>
        [Display(Name = "contact_tel")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string contact_tel { get; set; } = string.Empty;

        /// <summary>
        /// user's role
        /// </summary>
        [Display(Name = "user_role")]
        [Required(ErrorMessage = "Required")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string user_role { get; set; } = string.Empty;

        /// <summary>
        /// sex
        /// </summary>
        [Display(Name = "sex")]
        [MaxLength(10, ErrorMessage = "MaxLength")]
        public string sex { get; set; } = string.Empty;

        /// <summary>
        /// is_valid
        /// </summary>
        [Display(Name = "is_valid")]
        public bool is_valid { get; set; } = false;

        /// <summary>
        /// password
        /// </summary>
        [Display(Name = "password")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string auth_string { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        [Display(Name = "creator")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// createtime
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
        /// tenant
        /// </summary>
        [Display(Name = "tenant")]
        public long tenant_id { get; set; } = 0;

        #endregion property
    }
}