using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.Authen;


// generic response for login
public class LoginResponse
{
    public bool IsSuccess { get; set; }
    public int Code { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public LoginData? Data { get; set; }
}


// detailed login data
public class LoginData
{
    public string User_num { get; set; } = string.Empty;
    public string User_name { get; set; } = string.Empty;
    public int User_id { get; set; }
    public string User_role { get; set; } = string.Empty;
    public int Userrole_id { get; set; }
    public int Tenant_id { get; set; }
    public int Expire { get; set; }
    public string Access_token { get; set; } = string.Empty;
    public string Refresh_token { get; set; } = string.Empty;
}


// request payload for login
public class LoginRequest
{
    public string User_name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ClientEnvironment? Environment { get; set; }
}

