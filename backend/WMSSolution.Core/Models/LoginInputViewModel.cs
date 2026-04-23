
using System.ComponentModel.DataAnnotations;
using WMSSolution.Shared.RBAC;
namespace WMSSolution.Core.Models;

/// <summary>
/// Register
/// </summary>
public class RegisterInputViewModel
{
    /// <summary>
    /// User Name
    /// </summary>
    [Display(Name = "userName")]
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// Environment
    /// </summary>
    [Display(Name = "environment")]
    public ClientEnvironment? Environment { get; set; }
    /// <summary>
    /// Display Name
    /// </summary>
    [Display(Name = "displayName")]
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// login input viewmodel
/// </summary>
public class LoginInputViewModel
{
    /// <summary>
    /// username
    /// </summary>
    [Required(ErrorMessage = "Required")]
    [Display(Name = "user_name")]
    [MaxLength(128, ErrorMessage = "MaxLength")]
    public string user_name { get; set; } = string.Empty;

    /// <summary>
    /// password
    /// </summary>        
    [Display(Name = "password")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string password { get; set; } = string.Empty;
    /// <summary>
    /// Client Environment
    /// </summary>
    public ClientEnvironment? Environment { get; set; }
}
