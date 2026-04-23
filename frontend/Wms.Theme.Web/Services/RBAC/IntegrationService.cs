using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.RBAC;

namespace Wms.Theme.Web.Services.RBAC;

public class IntegrationService(IHttpClientFactory httpClientFactory,
    ILogger<UserService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IIntegrationService
{
    public async Task<IntegrationInfo> GetIntegrationInfo()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "user/integration-wcs";
            var response = await client.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            if (string.IsNullOrEmpty(responseContent)) return new();

            var result = JsonSerializer.Deserialize<ResultModel<IntegrationInfo>>(responseContent, jsonOptions);
            return result?.Data ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetIntegrationInfo");
            return new();
        }
    }

    public async Task<(bool isSuccess, string? message)> UpdateIntegrationInfo(IntegrationInfo request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "user/integration-wcs";
            var response = await client.PatchAsync(endpoint, request.ContentPretty());
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            if (string.IsNullOrEmpty(responseContent)) return new();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (false, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (true, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetIntegrationInfo");
            return new();
        }
    }
}
