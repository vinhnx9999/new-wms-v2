using System.Text.Json;
using Wms.Theme.Web.Model.OutboundGateway;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.OutboundGateway
{
    public class OutboundGatewayService(IHttpClientFactory httpClientFactory,
                 ILogger<OutboundGatewayService> logger, IConfiguration configuration) :
                 BaseApiService(httpClientFactory, logger, configuration), IOutboundGatewayService

    {

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new CustomDateTimeConverter()
            }
        };

        public async Task<(int id, string message)> AddAsync(AddOutboundGatewayRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/outbound-gateway";
                var response = await client.PostAsync(endpoint, request.ContentPretty());
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to add outbound gateway. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                    return (0, $"Failed to add outbound gateway. Status Code: {response.StatusCode}");
                }
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);
                return (result?.Data ?? 0, result?.ErrorMessage ?? "No message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync");
                return (0, "An error occurred while adding the outbound gateway.");
            }
        }

        public async Task<List<OutboundGatewayResponse>> PageSearchByWarehouse(PageSearchRequest pageSearch, int warehouseId)
        {
            try
            {
                var client = CreateClient();
                var endpoint = $"/outbound-gateway/list-by-warehouse?wareHouseId={warehouseId}";
                var response = await client.PostAsync(endpoint, pageSearch.ContentPretty());

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to page search outbound gateway. Status Code: {statusCode}", response.StatusCode);
                    return [];
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultModel<DataResponse<OutboundGatewayResponse>>>(content, jsonOptions);
                return result?.Data.Rows ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PageSearchByWarehouse");
                return [];
            }
        }
    }
}
