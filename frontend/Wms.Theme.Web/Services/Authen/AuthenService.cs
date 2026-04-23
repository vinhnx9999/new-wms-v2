using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.RBAC;
using RegisterRequest = WMSSolution.Shared.RBAC.RegisterRequest;

namespace Wms.Theme.Web.Services.Authen;

public class AuthenService(IHttpClientFactory httpClientFactory, 
    ILogger<AuthenService> logger, IConfiguration configuration) : 
    BaseApiService(httpClientFactory, logger, configuration), IAuthenService
{
    public async Task AuditUserAction(ClientEnvironment environment)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync("/audit-action", environment.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to ChangePassword data. Status Code: {statusCode} => {res}", response.StatusCode, response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during AuditUserAction user: {username} => {actionName}", environment.UserName, environment.ActionName);            
        }
    }

    public async Task<(int id, string message)> ChangePassword(string userName, string password, string newPassword)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/user/change-password";
            var request = new ChangePasswordRequest
            {
                Username = userName,
                Password = password,
                NewPassword = newPassword
            };

            var response = await client.PatchAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to ChangePassword data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API ChangePassword returned error: {errorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (0, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (resultModel.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ChangePassword");
            return (0, ex.Message);
        }
    }

    public async Task<LoginResponse> LoginAsync(string username, string password, ClientEnvironment? environment = null)
    {
        try
        {
            var client = CreateClient();
            var loginRequest = new LoginRequest
            {
                User_name = username,
                Password = password,
                Environment = environment 
            };

            var response = await client.PostAsJsonAsync("/login", loginRequest);
            var jsonString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Login response: {Response}", jsonString);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(jsonString, jsonOptions);
                return loginResponse ?? new LoginResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to login"
                };
            }
            else
            {
                return new LoginResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Login failed",
                    Code = (int)response.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {username}", username);
            return new LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync("/register", request.ContentPretty());
            var jsonString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Register response: {response}", jsonString);

            if (response.IsSuccessStatusCode)
            {
                var res = JsonSerializer.Deserialize<bool>(jsonString, jsonOptions);
                return res;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Register for user: {username}", request.UserName);
            return false;
        }
    }
}