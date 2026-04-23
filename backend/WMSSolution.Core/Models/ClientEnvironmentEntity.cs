using System.ComponentModel.DataAnnotations.Schema;

namespace WMSSolution.Core.Models;

/// <summary>
/// Client Environments
/// </summary>
[Table("ClientEnvironments")]
public class ClientEnvironmentEntity : BaseModel
{
    /// <summary>
    /// User Name
    /// </summary>
    public string? UserName { get; set; } = string.Empty;
    /// <summary>
    /// IP Address
    /// </summary>
    public string? IPAddress { get; set; } = string.Empty;
    /// <summary>
    /// Screen Width
    /// </summary>
    public int? ScreenWidth { get; set; } = 0;
    /// <summary>
    /// Screen Height
    /// </summary>
    public int? ScreenHeight { get; set; } = 0;
    /// <summary>
    /// Browser Info
    /// </summary>
    public string? BrowserInfo { get; set; } = string.Empty;
    /// <summary>
    /// Browser Version
    /// </summary>
    public string? BrowserVersion { get; set; } = string.Empty;
    /// <summary>
    /// OS Name
    /// </summary>
    public string? OSName { get; set; } = string.Empty;
    /// <summary>
    /// ActionName: Login / Logout / Access Denied / Unauthorized
    /// </summary>
    public string? ActionName { get; set; } = string.Empty;
    /// <summary>
    /// Created Time
    /// </summary>

    public DateTime? CreatedTime { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Creator
    /// </summary>
    public string? Creator { get; set; }
}
