using System.Text.Json;
using Wms.Theme.Web.Model.RBAC;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.RBAC;

public class UserService(IHttpClientFactory httpClientFactory,
    ILogger<UserService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IUserService
{
    public async Task<IEnumerable<UserDetailDTO>> GetAllUsersAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "user/user-details";
            var response = await client.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            if (string.IsNullOrEmpty(responseContent)) return [];

            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<UserDetailDTO>>>(responseContent, jsonOptions);
            var rs = (result?.Data ?? []);

            if (rs.Any())
            {
                return rs
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Username);
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllUsersAsync");
            return [];
        }
    }

    public Task<UserDetailDTO> GetUserByIdAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public async Task<(int? data, string? message)> ActiveUserAsync(int userId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/user/{userId}/active";
            var response = await client.PatchAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to active User data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API ActiveUser returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (0, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (resultModel.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ActiveUserAsync");
            return (0, ex.Message);
        }
    }

    public async Task<(int? data, string? message)> DeactiveUserAsync(int userId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/user/{userId}/de-active";
            var response = await client.PatchAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to de-active user data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API DeactiveUserAsync returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (0, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (resultModel.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeactiveUserAsync");
            return (0, ex.Message);
        }
    }

    public async Task<(int? data, string? message)> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/user/create-user";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create user data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API CreateUserAsync returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (0, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (resultModel.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateUserAsync");
            return (0, ex.Message);
        }
    }

    public async Task<(int? data, string? message)> UpdateUserInfo(UserDetailDTO request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/user/{request.Id}/update-user";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to update user data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API UpdateUserInfo returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (0, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (resultModel.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateUserInfo");
            return (0, ex.Message);
        }
    }

    public async Task<(int? data, string? message)> ResetUserPasswordAsync(int userId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/user/{userId}/reset-password";
            var response = await client.PatchAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to Reset User Password User data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API ResetUserPassword returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (0, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (resultModel.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ResetUserPassword");
            return (0, ex.Message);
        }
    }
}
