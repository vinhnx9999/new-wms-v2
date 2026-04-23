using System.Text.Json;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.GoodsOwner
{
    public class GoodsOwnerService : BaseApiService, IGoodOwnerService
    {
        public GoodsOwnerService(IHttpClientFactory httpClientFactory, ILogger<GoodsOwnerService> logger, IConfiguration configuration) : base(httpClientFactory, logger, configuration)
        {
        }

        public async Task<List<GoodOwnerDTO>> GetAllGoodOwner()
        {
            try
            {
                var client = CreateClient();
                var endpoint = "goodsowner/all";
                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get all goods locations. Status Code: {StatusCode}", response.StatusCode);
                    return new List<GoodOwnerDTO>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ListApiResponse<GoodOwnerDTO>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new CustomDateTimeConverter()
                    }
                });

                if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                else
                {
                    _logger.LogWarning("API returned unsuccessful response or null data. IsSuccess: {IsSuccess}, ErrorMessage: {ErrorMessage}",
                        apiResponse?.IsSuccess, apiResponse?.ErrorMessage);
                    return new List<GoodOwnerDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllGoodOwner");
                return new List<GoodOwnerDTO>();
            }
        }
    }
}
