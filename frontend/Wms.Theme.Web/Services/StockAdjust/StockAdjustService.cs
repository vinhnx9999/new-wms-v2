using System.Text.Json;
using Wms.Theme.Web.Entities.ViewModels;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockAdjust;
using Wms.Theme.Web.Model.StockProcess;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.StockAdjust;

public class StockAdjustService(IHttpClientFactory httpClientFactory, 
    ILogger<StockAdjustService> logger, 
    IConfiguration configuration) : 
    BaseApiService(httpClientFactory, logger, configuration), IStockAdjustServices
{
    private JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new CustomDateTimeConverter() }
    };

    public async Task<ResultModel<PageData<StockadjustViewModel>>> GetStockAdjustPageAsync(PageSearchRequest pageRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust/list";
            var response = await client.PostAsync(endpoint, pageRequest.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return new ResultModel<PageData<StockadjustViewModel>>();
            }
            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<PageData<StockadjustViewModel>>>(result, _jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                _logger.LogError("Result with response is null or not Sucess");
                return new ResultModel<PageData<StockadjustViewModel>>();
            }

            return resultResponse;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception {error}", ex.Message);
            return new ResultModel<PageData<StockadjustViewModel>>();
        }
    }

    public async Task<ResultModel<string>> CreateStockAdjustAsync(StockAdjustRequest model)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust";
            var response = await client.PostAsync(endpoint, model.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return new ResultModel<string>();
            }
            var result = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var responseResult = JsonSerializer.Deserialize<ResultModel<string>>(result, options);
            if (responseResult == null || !responseResult.IsSuccess)
            {
                _logger.LogError("Result with response is null or not Success");
                return new ResultModel<string>();
            }

            return responseResult;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception {error}", ex.Message);
            return new ResultModel<string>();
        }
    }

    public async Task<ResultModel<PageData<StockSourceSelectionViewModel>>> GetStockSourcesForChangeRequestAsync(PageSearchRequest searchRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust/stocksources";
            var response = await client.PostAsync(endpoint, searchRequest.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success");
            }
            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<PageData<StockSourceSelectionViewModel>>>(result, _jsonOptions);
            return resultResponse ?? throw new Exception("Response Is not Serialized");
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception : {ex}", ex);
            return new ResultModel<PageData<StockSourceSelectionViewModel>>();
        }
    }

    public async Task<ResultModel<PageData<SkuAdjustmentSelectionViewModel>>> GetSkuForAdjustmentSelectionAsync(PageSearchRequest searchRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust/sku-selection";
            var response = await client.PostAsync(endpoint, searchRequest.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success");
            }
            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<PageData<SkuAdjustmentSelectionViewModel>>>(result, _jsonOptions);
            return resultResponse ?? throw new Exception("Response Is not Serialized");
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception : {ex}", ex);
            return new ResultModel<PageData<SkuAdjustmentSelectionViewModel>>();
        }
    }

    public async Task<StockadjustViewModel?> GetDetailAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust/processing/?id=" + id;
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<StockadjustViewModel>>(result, _jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                _logger.LogWarning("[StockAdjust] Response error id={id}, ErrMsg = {error}", id, resultResponse?.ErrorMessage);
                return null;
            }

            return resultResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {error}", ex.Message);
            return null;
        }
    }

    public async Task<bool> UpdateProcessAsync(StockadjustViewModel request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockprocess";
            var response = await client.PutAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }
            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<bool>>(result, _jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                throw new Exception("ERROR: " + (resultResponse?.ErrorMessage ?? "Response error"));
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {error}", ex.Message);
            return false;
        }
    }

    public async Task<bool> ConfirmProcess(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust/process-confirm?id=" + id;
            var response = await client.PutAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }
            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<string>>(result, _jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                throw new Exception("ERROR: " + (resultResponse?.ErrorMessage ?? "Response error"));
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Confirm Process for {id}", id);
            return false;
        }
    }

    public async Task<bool> ConfirmAdjustment(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust/adjustment-confirm?id=" + id;
            var response = await client.PutAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }
            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<string>>(result, _jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                throw new Exception("ERROR: " + (resultResponse?.ErrorMessage ?? "Response error"));
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Confirm Adjustment for {id}", id);
            return false;
        }
    }

    public async Task<ResultModel<PageData<StockprocessGetViewModel>>> PageProcessingAsync(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockadjust/list-processing";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<PageData<StockprocessGetViewModel>>>(result, _jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                throw new Exception("ERROR: " + (resultResponse?.ErrorMessage ?? "Response error"));
            }
            return resultResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {error}", ex.Message);
            return new ResultModel<PageData<StockprocessGetViewModel>>();
        }
    }

    public async Task<string> DeleteStockProcessAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/stockadjust/processing/{id}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }
            var result = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<string>>(result, _jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                throw new Exception(resultResponse?.ErrorMessage ?? "Unknown error");
            }
            return resultResponse.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {error}", ex.Message);
            return "ERROR: " + ex.Message;
        }
    }

}
