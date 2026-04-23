using System.Text.Json;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Stock;

public class BeginMerchandiseService(IHttpClientFactory httpClientFactory,
    ILogger<StockService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IBeginMerchandiseService
{
    public async Task<(int? data, string? message)> DeleteBeginning(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/beginning/{id}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed when Delete Beginning");
                return (0, result?.ErrorMessage ?? "Deserialization failed");
            }
            return (result.Data, "success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Delete Beginning");
            return (0, ex.Message);
        }
    }

    public async Task<IEnumerable<BeginMerchandiseDto>> GetBeginMerchandises()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/beginning";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<IEnumerable<BeginMerchandiseDto>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {errorMessage}", resultModel?.ErrorMessage);
                return [];
            }

            return resultModel.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetBeginMerchandises");
            return [];
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<BeginMerchandiseExcel> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "receipt/beginning/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response beginning Import Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed beginning Import Excel ");
                return (0, "Deserialization Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API beginning Import Excel, Param : {param}", request);
            return (0, "Error occurred while calling API Import Excel is not success");
        }
    }

    public async Task<(int? data, string? message)> SaveBeginning()
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/beginning";
            var response = await client.PostAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not SaveBeginning");
                return (0, "Response is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError("API {Endpoint} returned error: {Error}",
                    endpoint, result?.ErrorMessage ?? "Deserialization failed when SaveBeginning");
                return (0, result?.ErrorMessage ?? "Deserialization failed");
            }
            return (result.Data, "success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Save Beginning");
            return (0, ex.Message);
        }
    }
}