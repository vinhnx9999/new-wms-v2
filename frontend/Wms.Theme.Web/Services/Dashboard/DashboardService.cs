using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Pages.Dashboard;
using WMSSolution.Shared.MasterData;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.Dashboard;
public class DashboardService(IHttpClientFactory httpClientFactory,
    ILogger<DashboardService> logger, IConfiguration configuration) : 
    BaseApiService(httpClientFactory, logger, configuration), IDashboardService
{
    public async Task<DashboardInfo> GetDashboardInfoAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/dashboard";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetDashboardInfoAsync. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return new();
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<DashboardInfo>>(content, jsonOptions);
            return result?.Data ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDashboardInfo");
        }

        return new();
    }

    public async Task<BaseUserInfo> GetUserInfo()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/my-info";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetUserInfo. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return new();
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<BaseUserInfo>>(content, jsonOptions);
            return result?.Data ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserInfo");
        }

        return new();
    }

    public async Task<MasterDataDto> LoadMasterData()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/dashboard/master-data";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetDashboardInfoAsync. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return new();
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<MasterDataDto>>(content, jsonOptions);
            return result?.Data ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDashboardInfo");
        }

        return new();
    }
}