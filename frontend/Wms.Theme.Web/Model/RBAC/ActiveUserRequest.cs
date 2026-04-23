namespace Wms.Theme.Web.Model.RBAC;

public class ActiveUserRequest
{
    public int UserId { get; set; }
}

public class CreateUserRequest
{
    public string UserName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string ContactTel { get; set; } = "";
}