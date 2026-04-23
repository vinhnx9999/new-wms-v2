using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text.Json;
using Wms.Theme.Web.Model.InboundReceipt;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Receipt;

public class ReceiptService(IHttpClientFactory httpClientFactory, 
    ILogger<ReceiptService> logger, IConfiguration configuration) : 
    BaseApiService(httpClientFactory, logger, configuration), IReceiptService
{
    public async Task<PageData<InboundReceiptListResponse>> PageSearchReceipt(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/inbound/list";
            var response = await client.PostAsync(endpoint, request.ContentPretty(base.jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<PageData<InboundReceiptListResponse>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {ErrorMessage}", resultModel?.ErrorMessage);
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

    public async Task<InboundReceiptDetailedDTO?> GetReceiptDetailAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/inbound/{id}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get receipt detail. Status Code: {StatusCode}", response.StatusCode);
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<InboundReceiptDetailedDTO>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return null;
            }

            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetReceiptDetailAsync for id={Id}", id);
            return null;
        }
    }

    public async Task<(int id, string message)> CreateNewReceiptAsync(CreateReceiptRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/inbound/create";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create receipt. Status Code: {StatusCode}", response.StatusCode);
                return (0, "Failed to create receipt");
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
            var endpoint = "/receipt/inbound/next-receipt-no";
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

    public async Task<bool> UpdateReceiptAsync(int id, UpdateInboundReceiptRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/inbound/{id}";
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

    public async Task<bool> CancelReceipt(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/cancel/{id}";
            var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to cancel receipt. Status Code: {StatusCode}", response.StatusCode);
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

            _logger.LogError(ex, "Error in CancelReceiptAsync for id={Id}", id);
            return false;
        }
    }

    public async Task<bool> RetryInboundTask(RetryInboundRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/inbound/retry";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retry inbound task. Status Code: {statusCode}", response.StatusCode);
                return false;
            }
            var content = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<bool>>(content, jsonOptions);

            return resultModel?.Data ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RetryInboundTaskAsync for receiptId={ReceiptId}", request.ReceiptId);
            return false;
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<InboundOrderExcel> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/inbound/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response Import Order Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization Import Order Excel is not success");
                return (0, "Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API Import Order Excel, Param : {param}", request);
            return (0, "Error occurred while calling API Import Excel is not success");
        }
    }

    public async Task<bool> RevertReceipt(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/revert/{id}";
            var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to revert receipt. Status Code: {StatusCode}", response.StatusCode);
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

            _logger.LogError(ex, "Error in Revert ReceiptAsync for id={Id}", id);
            return false;
        }
    }

    public async Task<bool> CloneReceipt(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/clone/{id}";
            var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to CloneReceipt. Status Code: {StatusCode}", response.StatusCode);
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

            _logger.LogError(ex, "Error in CloneReceipt for id={Id}", id);
            return false;
        }
    }

    public async Task<IEnumerable<InboundReceiptListResponse>> GetDeletedData()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/inbound/deleted-data";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<IEnumerable<InboundReceiptListResponse>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API Get inbound deleted data returned error: {errorMessage}", resultModel?.ErrorMessage);
                return [];
            }

            return resultModel.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Get inbound deleted data");
            return [];
        }
    }

    public async Task<(int id, string? message)> CreateNewBulkReceiptAsync(CreateBulkReceiptRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/receipt/inbound/create-multi-pallets";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to CreateNewBulkReceiptAsync. Status Code: {StatusCode}", response.StatusCode);
                return (0, "Failed to create receipt");
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
            _logger.LogError(ex, "Error in CreateNewBulkReceiptAsync");
            return (0, ex.Message);
        }
    }

    public async Task<string> GetShareInbound(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/receipt/share-inbounds/{id}";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to GetShareInbound. Status Code: {StatusCode}", response.StatusCode);
                return "";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return "";
            }

            return resultModel.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetShareInbound");
            return "";
        }
    }

    public async Task<InboundReceiptDetailedDTO?> GetReceiptSharingUrl(string sharingUrl)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/orders/{sharingUrl}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get receipt SharingUrl. Status Code: {statusCode}", response.StatusCode);
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<InboundReceiptDetailedDTO>>(responseContent, jsonOptions);
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
