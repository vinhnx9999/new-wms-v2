namespace WMSSolution.Shared.RBAC;

public class IntegrationInfo
{
    public string ApiUrl { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
}

public class BaseUserInfo
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>
    /// Role
    /// </summary>
    public string Role { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the display name associated with the object.
    /// </summary>
    /// <remarks>This property is typically used to provide a user-friendly name that can be displayed in user
    /// interfaces or reports.</remarks>
    public string DisplayName { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// User Detail
/// </summary>
public class UserDetailDTO: BaseUserInfo
{    
    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// IsActive
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Cell Phone
    /// </summary>
    public string CellPhone { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the object can be activated.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, the object is considered active and can perform its
    /// intended functions. Use this property to control the activation state of the object in application
    /// logic.</remarks>
    public bool CanActive { get; set; } = false;
    /// <summary>
    /// Gets or sets a value indicating whether the item can be deleted.
    /// Only for sysadmin
    /// </summary>
    /// <remarks>This property is typically used to control the visibility of delete options in the user
    /// interface. If set to <see langword="true"/>, the delete action is permitted; otherwise, it is
    /// restricted.</remarks>
    public bool CanDelete { get; set; } = false;

    public bool CanChangeRole
    {
        get
        {
            return UserRoleDef.IsAdminRole(Role);
        }
    }
}
