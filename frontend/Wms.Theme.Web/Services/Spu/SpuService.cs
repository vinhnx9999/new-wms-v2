
using System.Text;
using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.SPU;
using Wms.Theme.Web.Model.Stock;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.Spu
{
    public class SpuService : BaseApiService, ISpuService
    {
        public SpuService(IHttpClientFactory httpClientFactory, ILogger<SpuService> logger, IConfiguration configuration) : base(httpClientFactory, logger, configuration)
        {
        }

        public async Task<ApiResponse<SpuDto>?> getSpuListAsync(ListPageModelRequest request)
        {
            try
            {
                var client = CreateClient();

                // Get API endpoint
                var apiEndpoint = "spu/list";

                // Serialize the request to JSON
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


                // Make the POST request
                var response = await client.PostAsync(apiEndpoint, content);

                // Read response content regardless of status code
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<SpuDto>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new CustomDateTimeConverter() }
                });

                if (response.IsSuccessStatusCode && apiResponse?.IsSuccess == true)
                {
                    _logger.LogInformation("Successfully retrieved {Count} SPU records",
                        apiResponse.Data?.Rows?.Count ?? 0);
                }
                else
                {
                    _logger.LogWarning("API returned error: {ErrorMessage}. Status: {StatusCode}",
                        apiResponse?.ErrorMessage ?? "Unknown error", response.StatusCode);
                }
                return apiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getSpuListAsync");
                return null;
            }
        }

        public async Task<List<SkuUomDTO>> GetSkuUomListAsync()
        {
            try
            {
                var client = CreateClient();
                var endpoint = "spu/sku-uom-list";
                var response = await client.PostAsync(endpoint, null);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get Sku Uom List. Status Code: {StatusCode} => {res}", response.StatusCode, response);
                    throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultModel<List<SkuUomDTO>>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new CustomDateTimeConverter()
                    }
                });

                if (result == null || !result.IsSuccess)
                {
                    _logger.LogError("Failed to deserialize Sku Uom List or API returned an error. {rs}", result);
                    return [];
                }
                return result?.Data ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSkuUomListAsync");
                return [];
            }
        }
    }
}