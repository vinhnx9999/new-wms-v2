using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockMove;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.StockMove
{
    public class StockMoveService : BaseApiService, IStockMoveService
    {
        public StockMoveService(IHttpClientFactory httpClientFactory, ILogger<StockMoveService> logger, IConfiguration configuration) : base(httpClientFactory, logger, configuration)
        {
        }

        public async Task<ResultModel<PageData<StockmoveViewModel>>> GetStockPageAsync(PageSearchRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stockmove/list";
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Response not success");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var result = JsonSerializer.Deserialize<ResultModel<PageData<StockmoveViewModel>>>(responseContent, jsonOptions);
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result != null ? result.ErrorMessage : "Deserialization failed");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception :" + ex);
                return null;
            }
        }

        public async Task<StockmoveViewModel> GetStockMoveByIdAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stockmove?id=" + id;
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Response not success");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var result = JsonSerializer.Deserialize<ResultModel<StockmoveViewModel>>(responseContent, jsonOptions);
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result != null ? result.ErrorMessage : "Deserialization failed");
                }
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception :" + ex);
                return null;
            }
        }

        public async Task<bool> CreateStockMoveAsync(StockmoveViewModel stockMove)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stockmove";
                var content = new StringContent(JsonSerializer.Serialize(stockMove), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Response not success");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result != null ? result.ErrorMessage : "Deserialization failed");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception :" + ex);
                return false;
            }
        }

        public async Task<bool> ConfirmStockMoveAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stockmove/confirm?id=" + id;
                var stringContent = new StringContent("", System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync(endpoint, stringContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Response not success");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var result = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result != null ? result.ErrorMessage : "Deserialization failed");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception :" + ex);
                return false;
            }

        }

        public async Task<bool> RemoveStockMoveAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var endpoint = $"/stockmove?id={id}";
                var response = await client.DeleteAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Response not success");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var result = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, jsonOptions);
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result != null ? result.ErrorMessage : "Deserialization failed");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception :" + ex);
                return false;
            }
        }

        public async Task<StockMoveDashboardStats> GetDashboardStatsAsync()
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stockmove/dashboard-stats";
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    return new StockMoveDashboardStats();
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var result = JsonSerializer.Deserialize<ResultModel<StockMoveDashboardStats>>(responseContent, jsonOptions);
                if (result == null || !result.IsSuccess)
                {
                    return new StockMoveDashboardStats();
                }
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception :" + ex);
                return new StockMoveDashboardStats();
            }
        }
    }
}
