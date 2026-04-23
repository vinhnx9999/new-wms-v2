using System.Text.Json;
using Wms.Theme.Web.Model.PurchaseOrder;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.PurchaseOrder;

public class PurchaseOrderService(IHttpClientFactory httpClientFactory, ILogger<PurchaseOrderService> logger, IConfiguration configuration) : BaseApiService(httpClientFactory, logger, configuration), IPurchaseOrderService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new CustomDateTimeConverter() }
    };

    public async Task<ResultModel<PageData<PageSearchPOResponse>>> GetPageAsync(PageSearchRequest pageSearchRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "purchaseorder/list";
            var response = await client.PostAsync(endpoint, pageSearchRequest.ContentPretty(_jsonOptions));
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            var result = JsonSerializer.Deserialize<ResultModel<PageData<PageSearchPOResponse>>>(responseContent, _jsonOptions);
            if (result?.Data == null)
            {
                return result ?? new ResultModel<PageData<PageSearchPOResponse>>();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPageAsync");
            return new ResultModel<PageData<PageSearchPOResponse>>();
        }
    }

    public async Task<bool> CreateAsync(CreatePoRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "purchaseorder";
            var response = await client.PostAsync(endpoint, request.ContentPretty(_jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create purchase order. Status Code: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return false; // Indicating failure
            }
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            var result = JsonSerializer.Deserialize<ApiResult<int>>(responseContent, _jsonOptions);
            if (!result.IsSuccess)
            {
                _logger.LogError("API returned an error: {ErrorMessage}", result?.ErrorMessage);
                return false;
            }

            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"");
            return false;
        }
    }



    public async Task<ApiResult<bool>> UpdateAsync(CreateNewOrderRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "purchaseorder";
            var response = await client.PutAsync(endpoint, request.ContentPretty(_jsonOptions));
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";

            var result = JsonSerializer.Deserialize<ApiResult<bool>>(responseContent, _jsonOptions);
            return result ?? new ApiResult<bool> { IsSuccess = false, ErrorMessage = "Empty response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateAsync");
            return new ApiResult<bool> { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResult<string>> DeleteAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"purchaseorder?id={id}";
            var response = await client.DeleteAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            var result = JsonSerializer.Deserialize<ApiResult<string>>(responseContent, _jsonOptions);
            return result ?? new ApiResult<string> { IsSuccess = false, ErrorMessage = "Empty response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteAsync");
            return new ApiResult<string> { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResult<List<CreateNewOrderRequest>>> GetOpenPosAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "purchaseorder/open-list";
            var response = await client.GetAsync(endpoint);

            var responseContent = await response.Content.ReadAsStringAsync() ?? "";

            var result = JsonSerializer.Deserialize<ApiResult<List<CreateNewOrderRequest>>>(responseContent, _jsonOptions);
            _logger.LogInformation("GetOpenPosAsync Response: {result}", result);
            return result ?? new ApiResult<List<CreateNewOrderRequest>> { IsSuccess = false, ErrorMessage = "Empty response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOpenPosAsync");
            return new ApiResult<List<CreateNewOrderRequest>> { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResult<string>> CloseAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"purchaseorder/close?id={id}";

            // Sending PATCH request with empty content
            //var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            var result = JsonSerializer.Deserialize<ApiResult<string>>(responseContent, _jsonOptions);
            return result ?? new ApiResult<string> { IsSuccess = false, ErrorMessage = "Empty response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CloseAsync");
            return new ApiResult<string> { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<string> GeneratePoNo()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "purchaseorder/generate-po-no";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to generate PO number. Status: {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            var content = await response.Content.ReadAsStringAsync() ?? "";
            var result = JsonSerializer.Deserialize<ResultModel<string>>(content, _jsonOptions);

            if (result is null || !result.IsSuccess || string.IsNullOrWhiteSpace(result.Data))
            {
                _logger.LogError("GeneratePoNo response invalid");
                return string.Empty;
            }

            return result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GeneratePoNo");
            return string.Empty;
        }
    }

    public async Task<ApiResult<PoDetailDto>> GetDetailAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"purchaseorder/{id}";
            var response = await client.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync() ?? "";
            var result = JsonSerializer.Deserialize<ApiResult<PoDetailDto>>(responseContent, _jsonOptions);

            return result ?? new ApiResult<PoDetailDto>
            {
                IsSuccess = false,
                ErrorMessage = "Empty response"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDetailAsync");
            return new ApiResult<PoDetailDto>
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
