using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public RegisterViewModel()
        {

        }
        /// <summary>
        /// user's name
        /// </summary>
        [Display(Name = "user_name")]
        [Required(ErrorMessage = "Required")]
        [MaxLength(128, ErrorMessage = "MaxLength")]
        public string user_name { get; set; } = string.Empty;

        /// <summary>
        /// sex
        /// </summary>
        public string sex { get; set; } = string.Empty;

        /// <summary>
        /// password
        /// </summary>
        [Display(Name = "password")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string auth_string { get; set; } = string.Empty;

        /// <summary>
        /// email
        /// </summary>
        [Display(Name = "email")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string email { get; set; } = string.Empty;


    }
}
