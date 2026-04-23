using System.Text.Json;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.InboundPallet;

public class InboundPalletService(
    IHttpClientFactory httpClientFactory,
    ILogger<InboundPalletService> logger,
    IConfiguration configuration) : BaseApiService(httpClientFactory, logger, configuration), IInboundPalletService
{
    public async Task<(int id, string message)> CreateAsync(CreateInboundPalletRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync("/inbound-pallet/create", request.ContentPretty(jsonOptions), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Create inbound pallet failed. Status: {StatusCode}", response.StatusCode);
                return (0, "Create inbound pallet failed");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                return (0, result?.ErrorMessage ?? "Create inbound pallet failed");
            }

            return (result.Data, result.ErrorMessage ?? "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in InboundPalletService.CreateAsync");
            return (0, ex.Message);
        }
    }
}