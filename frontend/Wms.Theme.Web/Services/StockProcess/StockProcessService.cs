using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockProcess;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.StockProcess;

public class StockProcessService(IHttpClientFactory httpClientFactory, 
    ILogger<StockProcessService> logger, IConfiguration configuration) : 
    BaseApiService(httpClientFactory, logger, configuration), IStockProcessService
{
    public async Task<string> AddAsync(StockprocessViewModel request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockprocess";
            var response = await client.PostAsync(endpoint, request.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };

            var resultResponse = JsonSerializer.Deserialize<ResultModel<int>>(result, jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                return "ERROR: " + (resultResponse?.ErrorMessage ?? "Unknown error");
            }
            return resultResponse.Data.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {error}", ex.Message);
            return "ERROR: " + ex.Message;
        }
    }

    public async Task<ResultModel<PageData<StockprocessGetViewModel>>> PageAsync(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockprocess/list";
            var response = await client.PostAsync(endpoint, request.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };

            var resultResponse = JsonSerializer.Deserialize<ResultModel<PageData<StockprocessGetViewModel>>>(result, jsonOptions);
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
            var endpoint = "/stockprocess?id=" + id;
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }
            var result = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };
            var resultResponse = JsonSerializer.Deserialize<ResultModel<string>>(result, jsonOptions);
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

    public async Task<StockprocessWithDetailViewModel> GetDetailAsync(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = " /stockprocess/?id=" + id;
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };

            var resultResponse = JsonSerializer.Deserialize<ResultModel<StockprocessWithDetailViewModel>>(result, jsonOptions);
            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                throw new Exception("ERROR: " + (resultResponse?.ErrorMessage ?? "Response error"));
            }

            return resultResponse.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {error}", ex.Message);
            return new StockprocessWithDetailViewModel();
        }
    }
    
    public async Task<bool> UpdateProcessAsync(StockprocessViewModel request)
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
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };
            var resultResponse = JsonSerializer.Deserialize<ResultModel<bool>>(result, jsonOptions);
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

    public async Task<StockProcessDashboardStatsViewModel> GetDashboardStatsAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockprocess/dashboard-stats";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };

            var resultResponse = JsonSerializer.Deserialize<ResultModel<StockProcessDashboardStatsViewModel>>(result, jsonOptions);

            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                // Decide whether to throw or return default
                _logger.LogError("Error fetching dashboard status: {error}", (resultResponse?.ErrorMessage ?? "Unknown error"));
                return new StockProcessDashboardStatsViewModel();
            }

            return resultResponse.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetDashboardStatsAsync: {error}", ex.Message);
            return new StockProcessDashboardStatsViewModel();
        }
    }

    public async Task<bool> ConfirmProcess(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stockprocess/process-confirm?id=" + id;
            var response = await client.PutAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }
            var result = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };
            var resultResponse = JsonSerializer.Deserialize<ResultModel<string>>(result, jsonOptions);
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
            var endpoint = "/stockprocess/adjustment-confirm?id=" + id;
            var response = await client.PutAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response is not success: " + response.StatusCode);
            }
            var result = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            };
            var resultResponse = JsonSerializer.Deserialize<ResultModel<string>>(result, jsonOptions);
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
}
