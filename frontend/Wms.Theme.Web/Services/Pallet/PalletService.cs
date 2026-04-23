using System.Text.Json;
using Wms.Theme.Web.Model.Pallet;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.Pallet
{
    public class PalletService : BaseApiService, IPalletService
    {
        public PalletService(IHttpClientFactory httpClientFactory, ILogger<PalletService> logger, IConfiguration configuration)
            : base(httpClientFactory, logger, configuration)
        {
        }

        public async Task<ResultModel<List<PalletDto>>?> GetAllAsync()
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/pallet/all";
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get all pallets. Status Code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultModel<List<PalletDto>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new CustomDateTimeConverter()
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PalletService.GetAllAsync");
                return null;
            }
        }

        public async Task<CreatePalletResponse?> GetNextPalletCode()
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/pallet/generate-code";
                var response = await client.PostAsync(endpoint, null);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to generate pallet code. Status Code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultModel<CreatePalletResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true

                });

                if (result?.Code == 200)
                {
                    return result.Data;
                }

                _logger.LogWarning("Generate pallet code failed: {Message}", result?.ErrorMessage);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PalletService.GetNextPalletCode");
                return null;
            }
        }

        public async Task<ResultModel<PalletPageSearchResponse>?> SearchAsync(PageSearchRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/pallet/list";
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(endpoint, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to search pallets. Status Code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultModel<PalletPageSearchResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new CustomDateTimeConverter()
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PalletService.SearchAsync");
                return null;
            }
        }
    }
}

