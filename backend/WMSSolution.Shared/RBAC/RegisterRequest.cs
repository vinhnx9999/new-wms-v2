namespace WMSSolution.Shared.RBAC;

public class RegisterRequest
{
    public string DisplayName { get; set; } = "";
    public string UserName { get; set; } = string.Empty;
    public ClientEnvironment? Environment { get; set; }
    public string ContactNumber { get; set; }
    public string CompanyName { get; set; }
}