using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockTaking;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.StockTaking
{
    public class StockTakingService : BaseApiService, IStockTakingService
    {
        public StockTakingService(IHttpClientFactory httpClientFactory, ILogger<StockTakingService> logger, IConfiguration configuration) : base(httpClientFactory, logger, configuration)
        {
        }

        public async Task<ResultModel<PageData<StocktakingViewModel>>> GetStockTakingPageAsync(PageSearchRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stocktaking/list";
                var stringContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, stringContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to retrieve stock taking data");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<PageData<StocktakingViewModel>>>(responseContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result?.ErrorMessage ?? "Failed to retrieve stock taking data");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStockTakingPageAsync");
                return new ResultModel<PageData<StocktakingViewModel>>();
            }
        }

        public async Task<bool> AddStockTakingAsync(StocktakingBasicViewModel request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stocktaking";
                var stringContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, stringContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to add stock taking record");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(responseContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });

                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result?.ErrorMessage ?? "Failed to add stock taking record");
                }
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddStockTakingAsync");
                return false;
            }
        }

        public async Task<bool> CountedStockTakingAsync(StocktakingConfirmViewModel request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stocktaking";
                var stringContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync(endpoint, stringContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to confirm stock taking count");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result?.ErrorMessage ?? "Failed to confirm stock taking count");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CountedStockTakingAsync");
                return false;
            }
        }

        public async Task<bool> RemoveStockTakingAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stocktaking?id=" + id;
                var response = await client.DeleteAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to remove stock taking record");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<string>>(responseContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result?.ErrorMessage ?? "Failed to remove stock taking record");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveStockTakingAsync");
                return false;
            }
        }

        public async Task<bool> ConfirmStockTakingAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stocktaking/adjustment-confirm?id=" + id;
                var stringContent = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync(endpoint, stringContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to confirm stock taking adjustment");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception("Failed to confirm stock taking adjustment");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ConfirmStockTakingAsync" + ex.Message);
                return false;
            }
        }

        public async Task<StocktakingViewModel> GetStockTakingById(int id)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/stocktaking?id=" + id;
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to retrieve stock taking record");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<StocktakingViewModel>>(responseContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (result == null || !result.IsSuccess)
                {
                    throw new Exception(result?.ErrorMessage ?? "Failed to retrieve stock taking record");
                }
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStockTakingById");
                return new StocktakingViewModel();
            }
        }
    }
}
