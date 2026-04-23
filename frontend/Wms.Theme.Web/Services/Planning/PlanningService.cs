using System.Text.Json;
using Wms.Theme.Web.Model.Planning;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.Planning;

public class PlanningService(IHttpClientFactory httpClientFactory,
    ILogger<SkuService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IPlanningService
{
    public async Task<IEnumerable<PickingDTO>> GetPackingList()
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/planning/packing";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<PickingDTO>>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError("API {Endpoint} returned error: {Error}",
                    endpoint, result?.ErrorMessage ?? "Deserialization failed PickingList");
                return [];
            }
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API PickingList");
            return [];
        }
    }

    public async Task<IEnumerable<PickingDTO>> GetPickingList()
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/planning/picking";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<PickingDTO>>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError("API {Endpoint} returned error: {Error}",
                    endpoint, result?.ErrorMessage ?? "Deserialization failed PickingList");
                return [];
            }
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API PickingList");
            return [];
        }
    }

    public async Task<(bool Success, string? Message)> SavePickingList(IEnumerable<PickingDTO> requests)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/planning/picking";
            var resposne = await client.PostAsync(endpoint, requests.ContentPretty());
            if (!resposne.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (false, "Response is not success");
            }
            var responseContent = await resposne.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (false, "Deserialization failed");
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", requests);
            return (false, "Error occurred while calling API");
        }
    }
}
