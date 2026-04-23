using System.ComponentModel.DataAnnotations;

namespace WMSSolution.Core.Models
{
    /// <summary>
    /// RefreshTokenInPutViewModel
    /// </summary>
    public class RefreshTokenInPutViewModel
    {
        /// <summary>
        /// old access token
        /// </summary>
        [Required(ErrorMessage = "AccessToken  is Required")]
        public required string AccessToken { get; set; }
        /// <summary>
        /// refresh token
        /// </summary>
        [Required(ErrorMessage = "RefreshToken is Required")]
        public required string RefreshToken { get; set; }

    }
}
