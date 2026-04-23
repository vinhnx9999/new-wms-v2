using DocumentFormat.OpenXml.Office2016.Excel;
using System.Text.Json;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.AsnMaster;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Receipt;

public class OutboundReceiptService(IHttpClientFactory httpClientFactory,
                                    ILogger<AsnMasterService> logger,
                                    IConfiguration configuration) : 
    BaseApiService(httpClientFactory, logger, configuration), IOutboundReceiptService
{
    public async Task<bool> CancelReceipt(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/outbound/cancel/{id}";
            var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to cancel outbound receipt. Status Code: {StatusCode}", response.StatusCode);
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);

            return resultModel?.Data ?? false;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error in CancelReceiptAsync for ID {Id}", id);
            return false;
        }
    }

    public async Task<bool> CloneReceipt(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/outbound/clone/{id}";
            var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to Clone outbound receipt. Status Code: {StatusCode}", response.StatusCode);
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);

            return resultModel?.Data ?? false;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error in CloneReceipt outbound for ID {Id}", id);
            return false;
        }
    }

    public async Task<(int id, string message)> CreateOutboundReceiptAsync(CreateOutboundReceiptRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/outbound/create";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create outbound receipt. Status Code: {StatusCode}", response.StatusCode);
                return (0, "Failed to create outbound receipt");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return (0, resultModel?.ErrorMessage ?? "Unknown error");
            }

            return (resultModel.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateReceiptAsync");
            return (0, ex.Message);
        }
    }

    public async Task<string> GetNextReceiptCode()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/outbound/next-receipt-no";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get next receipt code. Status Code: {StatusCode}", response.StatusCode);
                return string.Empty;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return string.Empty;
            }
            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNextReceiptCodeAsync");
            return string.Empty;
        }
    }

    public async Task<OutboundReceiptDetailedDto?> GetReceiptDetailAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/outbound/{id}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get receipt detail for ID {Id}. Status Code: {StatusCode}", id, response.StatusCode);
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<OutboundReceiptDetailedDto>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return null;
            }
            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetReceiptDetailAsync for ID {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<OutboundReceiptListResponse>> GetDeletedData()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/outbound/deleted-data";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<IEnumerable<OutboundReceiptListResponse>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API GetRevertData returned error: {errorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return [];
            }

            return resultModel?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRevertData");
            return [];
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<OutboundOrderExcel> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/outbound/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response Import outbound Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization Import outbound Excel is not success");
                return (0, "Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API Import outbound Excel, Param : {param}", request);
            return (0, "Error occurred while calling API Import Excel is not success");
        }
    }

    public async Task<PageData<OutboundReceiptListResponse>> PageSearchReceipt(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/outbound/list";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<PageData<OutboundReceiptListResponse>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return null;
            }

            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PageSearchReceipt");
            return null;
        }
    }

    public async Task<bool> RevertReceipt(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/outbound/revert/{id}";
            var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to revert outbound receipt. Status Code: {StatusCode}", response.StatusCode);
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);

            return resultModel?.Data ?? false;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error in outbound RevertReceipt for ID {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateReceiptAsync(int id, UpdateOutboundReceiptRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/outbound/{id}";
            var response = await client.PutAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to update receipt. Status Code: {StatusCode}", response.StatusCode);
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return false;
            }

            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateReceiptAsync");
            return false;
        }
    }

    public async Task<string?> GetShareOutbound(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/share-outbounds/{id}";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to GetShareOutbound. Status Code: {StatusCode}", response.StatusCode);
                return "";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API returned error: {errorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return "";
            }

            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetShareOutbound");
            return "";
        }
    }

    public async Task<OutboundReceiptDetailedDto> GetReceiptSharingUrl(string sharingUrl)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/orders/sales/{sharingUrl}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get receipt SharingUrl. Status Code: {statusCode}", response.StatusCode);
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<OutboundReceiptDetailedDto>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {errorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return null;
            }

            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetReceiptSharingUrl for id={sharingUrl}", sharingUrl);
            return null;
        }
    }
}
