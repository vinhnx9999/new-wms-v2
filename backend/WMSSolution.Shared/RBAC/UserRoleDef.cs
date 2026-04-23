namespace WMSSolution.Shared.RBAC;

/// <summary>
/// User Role Def
/// </summary>
public static class UserRoleDef
{
    /// <summary>
    /// System Administrator
    /// Full control over WMS configuration, integrations, and user management
    /// </summary>
    public static string SystemAdministrator = "SysAdmin";
    /// <summary>
    /// Limit Admin accounts: Reduce risk by restricting full-access roles
    /// </summary>
    public static string Admin = "Admin";
    /// <summary>
    /// aministrator role is for the warehouse administrators 
    public static string Administrator = "administrator";
    /// <summary>
    /// Oversees warehouse operations and supervises staff.
    /// </summary>
    public static string Manager = "Manager";
    /// <summary>
    /// Directly manages operators and ensures compliance with processes.
    /// </summary>
    public static string Supervisor = "Supervisor";
    /// <summary>
    /// Executes day-to-day warehouse activities.
    /// </summary>
    public static string Operator = "Operator";
    /// <summary>
    /// Each level inherits fewer permissions than the one above, ensuring least privilege
    /// </summary>
    public static string Guest = "Guest";
    public static string ShowVendors = "ShowVendors";

    public static List<string> AllRoles =
    [
        SystemAdministrator,
        Admin,
        Manager,
        Supervisor,
        Operator,
        Guest
    ];

    public static List<string> AdminRoles =
    [
        SystemAdministrator,
        Administrator,
        Admin
    ];

    public static List<string> ManagerRoles =
    [
        SystemAdministrator,
        Admin,
        Manager
    ];


    public static bool IsSystemAdministrator(string role)
    {
        return SystemAdministrator == role;
    }

    public static bool IsAdminRole(string role)
    {
        return AdminRoles.Contains(role, StringComparer.InvariantCultureIgnoreCase);
    }

    public static bool IsManagerRoles(string role)
    {
        return ManagerRoles.Contains(role, StringComparer.InvariantCultureIgnoreCase);
    }
}
