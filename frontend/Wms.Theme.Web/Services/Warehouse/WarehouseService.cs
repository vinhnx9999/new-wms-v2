using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.Planning;

namespace Wms.Theme.Web.Services.Warehouse;

public class WarehouseService(IHttpClientFactory httpClientFactory,
    ILogger<WarehouseService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IWarehouseService
{
    public async Task<List<FormSelectItem>> GetSelectItemsAsnyc()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/warehouse/select-item";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("failed to fetch warehouse select items. Status Code: {StatusCode}", response.StatusCode);
                return [];
            }
            var resultContent = await response.Content.ReadAsStringAsync();
            var responseContent = JsonSerializer.Deserialize<ResultModel<List<FormSelectItem>>>(resultContent, jsonOptions);
            return responseContent?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSelectItemsAsnyc");
            throw;
        }
    }

    public async Task<ResultModel<List<WarehouseViewModel>>> GetAllAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/warehouse/all";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get all warehouses. Status Code: {StatusCode}", response.StatusCode);
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<WarehouseViewModel>>>(content, jsonOptions);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllAsync");
            return null;
        }
    }

    public async Task<IEnumerable<WcsLocationDto>> GetWcsLocationsAsnyc(string wcsBlockId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/locations/map-locations/{wcsBlockId}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get all map locations of warehouses. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<WcsLocationDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetWcsLocationsAsnyc");
            return [];
        }
    }
    public async Task<IEnumerable<WcsLocationDto>> GetWcsLocationsAsnyc(Guid? guid = null)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/locations/map-locations/{guid}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get all map locations of warehouses. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<WcsLocationDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetWcsLocationsAsnyc");
            return [];
        }
    }

    //create-rule-settings
    public async Task<(int Id, string Message)> CreateSettingsAsync(CreateStoreSettingsRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/warehouse/create-rule-settings";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create-rule-settings data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
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
            _logger.LogError(ex, "Error in CreateSettingsAsync");
            return (0, ex.Message);
        }
    }

    //get-rule-settings
    public async Task<IEnumerable<StoreRuleSettingsDto>> GetSettingsAsync(int storeId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"warehouse/{storeId}/rule-settings";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get all rule settings of warehouses. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<StoreRuleSettingsDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSettingsAsync");
            return [];
        }
    }

    public async Task<bool> DeleteRuleSettings(int storeId, int settingRuleId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"warehouse/{storeId}/rule-settings/{settingRuleId}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to DeleteRuleSettings of warehouses. Status Code: {statusCode}", response.StatusCode);
                return false;
            }
            var content = await response.Content.ReadAsStringAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteRuleSettings");
            return false;
        }
    }

    public async Task<(int Id, string Message)> SaveWcsLocations(int storeId, 
        IEnumerable<WcsLocationDto> wcsLocations, string blockId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"warehouse/{storeId}/synchronous-wcs-locations";
            var request = new
            {
                WarehouseId = storeId,
                Locations = wcsLocations,
                WcsBlockId = blockId
            };

            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to synchronous-wcs-locations data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
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
            _logger.LogError(ex, "Error in SaveWcsLocations");
            return (0, ex.Message);
        }
    }

    public async Task<IEnumerable<WarehouseVM>> PageSearchWarehouse(PageSearchRequest pageSearchRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/warehouse/list";
            var response = await client.PostAsync(endpoint, pageSearchRequest.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to page search warehouse. Status Code: {statusCode} => {res}", response.StatusCode, response);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<DataResponse<WarehouseVM>>>(content, jsonOptions);
            return result?.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PageSearchWarehouse");
            return [];
        }
    }

    /// <summary>
    /// Add warehouse
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<(int? Id, string? Message)> AddAsync(AddWareHouseRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/warehouse";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to add warehouse. Status Code: {statusCode} => {res}", response.StatusCode, response);
                return (null, "");
            }
            var content = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);
            return (resultModel?.Data, resultModel?.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddAsync");
            return (null, ex.Message);
        }
    }

    public async Task<(int? data, string? message)> ActiveWareHouse(int wareHouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{wareHouseId}/active";
            var response = await client.PatchAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to active warehouse data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
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
            _logger.LogError(ex, "Error in CreateSettingsAsync");
            return (0, ex.Message);
        }
    }

    public async Task<(int? data, string? message)> DeActiveWareHouse(int wareHouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{wareHouseId}/de-active";
            var response = await client.PatchAsync(endpoint, "".ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to de-active warehouse data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
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
            _logger.LogError(ex, "Error in CreateSettingsAsync");
            return (0, ex.Message);
        }
    }

    /// <summary>
    /// General Info
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <returns></returns>
    public async Task<WarehouseGeneralInfo> GetGeneralInfoAsync(int wareHouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{wareHouseId}/general-info";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get all map locations of warehouses. Status Code: {statusCode}", response.StatusCode);
                return new();
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<WarehouseGeneralInfo>>(content, jsonOptions);
            return result?.Data ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetWcsLocationsAsnyc");
            return new();
        }
    }

    public async Task<IEnumerable<InboundInfoModel>> GetInboundByWarehouse(int warehouseId, PageSearchRequest condition)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{warehouseId}/receipt-details";
            var response = await client.PostAsync(endpoint, condition.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get InboundByWarehouse. Status Code: {StatusCode}", response.StatusCode);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<IEnumerable<InboundInfoModel>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API returned error: {ErrorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return [];
            }
            return resultModel?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in InboundByWarehouse");
            return [];
        }
    }

    public async Task<IEnumerable<OutboundInfoModel>> GetOutboundByWarehouse(int warehouseId, PageSearchRequest condition)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{warehouseId}/order-details";
            var response = await client.PostAsync(endpoint, condition.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get OutboundInfoModel. Status Code: {StatusCode}", response.StatusCode);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<IEnumerable<OutboundInfoModel>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API OutboundInfoModel returned error: {errorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return [];
            }
            return resultModel?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OutboundInfoModel");
            return [];
        }
    }

    public async Task<IEnumerable<InventoryOverview>> GetInventoryOverview(int warehouseId, PageSearchRequest condition)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{warehouseId}/inventory-overviews";
            var response = await client.PostAsync(endpoint, condition.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get InventoryOverviewByWarehouse. Status Code: {StatusCode}", response.StatusCode);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            var resultModel = JsonSerializer.Deserialize<ResultModel<IEnumerable<InventoryOverview>>>(responseContent, jsonOptions);
            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("Failed to deserialize or API InventoryOverview returned error: {errorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return [];
            }
            return resultModel?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in InventoryOverview");
            return [];
        }
    }

    public async Task<IEnumerable<SkuSafetyStockDto>> GetSafetyStockConfig(int warehouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{warehouseId}/safety-stock-config";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get all GetSafetyStockConfig. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<SkuSafetyStockDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSafetyStockConfig");
            return [];
        }
    }

    public async Task<(int? data, string? message)> ImportExcelSafetyStock(int warehouseId, 
        List<InputSkuSafetyStock> inputSkuSafety)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{warehouseId}/import-safety-stock-config";
            var response = await client.PostAsync(endpoint, inputSkuSafety.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to Import Excel Safety Stock Config. " +
                    "Status Code: {statusCode}", response.StatusCode);
                return (0, "Failed to Import Excel");
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);
            return (1, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Import Excel Safety Stock Config {error}", ex.Message);
            return (0, ex.Message ?? "Failed to Import Excel"); 
        }
    }

    public async Task<(int? data, string? message)> UpdateSkuSafetyStock(int warehouseId, SkuSafetyStockDto skuSafety)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{warehouseId}/safety-stock-config";
            var response = await client.PatchAsync(endpoint, skuSafety.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to Update Safety Stock Config. " +
                    "Status Code: {statusCode}", response.StatusCode);
                return (0, "Failed to Update Safety Stock Config");
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);
            return (1, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Update Safety Stock Config {error}", ex.Message);
            return (0, ex.Message ?? "Failed to Update Safety Stock Config");
        }
    }

    public async Task<(int? data, string? message)> DeleteSafetyData(int warehouseId, int skuSafetyId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/warehouse/{warehouseId}/safety-stock-config/{skuSafetyId}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to Delete Safety Stock Config. " +
                    "Status Code: {statusCode}", response.StatusCode);
                return (0, "Failed to Delete Safety Stock Config");
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);
            return (1, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Delete Safety Stock Config {error}", ex.Message);
            return (0, ex.Message ?? "Failed to Delete Safety Stock Config");
        }
    }

    public async Task<IEnumerable<AvailablePallet>> CalculatorPallets(CalculatorPalletRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"warehouse/calculator-pallets";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to CalculatorPallets data. Status Code: {statusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var resultModel = JsonSerializer.Deserialize<ResultModel<IEnumerable<AvailablePallet>>>(responseContent, jsonOptions);

            if (resultModel == null || !resultModel.IsSuccess)
            {
                _logger.LogError("API returned error: {errorMessage}", resultModel?.ErrorMessage ?? "Unknown error");
                return [];
            }

            return resultModel?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CalculatorPallets {error}", ex.Message);
            return [];
        }
    }

    public async Task<IEnumerable<int>> GetFloors(int warehouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"warehouse/{warehouseId}/floors";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get all floors of warehouses. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<int>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetFloors {warehouseId} has {error}", warehouseId, ex.Message);
            return [];
        }
    }
}
