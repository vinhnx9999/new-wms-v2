using System.Text.Json;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.GoodLocation;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Stock;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.Stock;

public class StockService(IHttpClientFactory httpClientFactory,
    ILogger<StockService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IStockService
{
    public async Task<List<SkuSelectDTO>> GetSkuSelect(PageSearchRequest pageRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "stock/sku-select";
            var response = await client.PostAsync(endpoint, pageRequest.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get SKU select data. Status Code: {StatusCode} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var skuSelectList = JsonSerializer.Deserialize<PageSearchResponse<SkuSelectDTO>>(responseContent, jsonOptions);
            if (skuSelectList == null || !skuSelectList.IsSuccess)
            {
                _logger.LogError("Failed to deserialize SKU select data or API returned an error. {rs}", skuSelectList);
                return [];
            }
            return skuSelectList?.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSkuSelect");
            return [];
        }
    }

    public async Task<List<SkuSelectDTO>> GetSkuSelectAvailable(PageSearchRequest pageSearchRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "stock/sku-select-available";
            var response = await client.PostAsync(endpoint, pageSearchRequest.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get SKU select available data. Status Code: {code} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var skuSelectList = JsonSerializer.Deserialize<PageSearchResponse<SkuSelectDTO>>(responseContent, jsonOptions);
            if (skuSelectList == null || !skuSelectList.IsSuccess)
            {
                _logger.LogError("Failed to deserialize SKU select available data or API returned an error. {rs}", skuSelectList);
                return [];
            }
            return skuSelectList?.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSkuSelectAvailable");
            return [];
        }
    }

    public async Task<ResultModel<PageData<LocationStockManagementViewModel>>> LocationStockPageAsync(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/location-list";
            var response = await client.PostAsync(endpoint, request.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve location stock. Status code: {code} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<PageData<LocationStockManagementViewModel>>>(responseContent, jsonOptions);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize location stock response or operation was not successful. {result}", result);
                throw new Exception("Failed to retrieve location stock data.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving location stock page");
            return new ResultModel<PageData<LocationStockManagementViewModel>>();
        }
    }

    public async Task<ResultModel<InventoryDashboardViewModel>> GetInventoryDashboardAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/dashboard-stats";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve inventory dashboard. Status code: {code} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultResponse = JsonSerializer.Deserialize<ResultModel<InventoryDashboardViewModel>>(responseContent, jsonOptions);

            if (resultResponse == null || !resultResponse.IsSuccess)
            {
                _logger.LogError("Failed to retrieve inventory dashboard data. {res}", resultResponse);
                throw new Exception("Failed to retrieve inventory dashboard data.");
            }
            return resultResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetInventoryDashboardAsync");
            throw;
        }
    }

    public async Task<ResultModel<PageData<StockViewModel>>> GetSelectPageAsync(PageSearchRequest pageRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/select";
            var response = await client.PostAsync(endpoint, pageRequest.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve stock select page. Status code: {code} => {rs}", response.StatusCode, response);
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<PageData<StockViewModel>>>(responseContent, jsonOptions);
            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve stock select page data. {result}", result);
                throw new Exception("Failed to retrieve stock select page data.");
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving stock select page");
            return new ResultModel<PageData<StockViewModel>>();
        }
    }

    public async Task<ResultModel<PageData<ProductLifeCycleViewModel>>> GetProductLiftPageAsync(PageSearchRequest pageRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/product-lifecycle";
            var response = await client.PostAsync(endpoint, pageRequest.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve product lifecycle page. Status code: {code} => {res}", response.StatusCode, response);
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<PageData<ProductLifeCycleViewModel>>>(responseContent, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve product lifecycle page data. {rs}", result);
                throw new Exception("Failed to retrieve product lifecycle page data.");
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product lifecycle page");
            return new ResultModel<PageData<ProductLifeCycleViewModel>>();
        }
    }

    public async Task<ResultModel<PageData<StockManagementViewModel>>> StockPageAsync(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/stock-list";
            var response = await client.PostAsync(endpoint, request.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve stock list. Status code: {resCode} -> {response}", response.StatusCode, response);
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<PageData<StockManagementViewModel>>>(responseContent, jsonOptions);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize stock list response or operation was not successful.");
                throw new Exception("Failed to retrieve stock list data.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving stock list page");
            return new ResultModel<PageData<StockManagementViewModel>>();
        }
    }

    public async Task<List<StockLocationDTO>> GetStockLocationsAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/locations-available";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve stock locations. Status code: {code}", response.StatusCode);
                return [];
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ListApiResponse<StockLocationDTO>>(responseContent, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to deserialize stock locations response. {result}", result);
                return [];
            }

            return result.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving stock locations");
            return [];
        }
    }

    /// <summary>
    /// Search SKU with available quantity for outbound
    /// POST /api/stock/sku-available
    /// </summary>
    public async Task<ResultModel<PageData<SkuAvailableDTO>>> GetSkuAvailableAsync(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/sku-available";
            var response = await client.PostAsync(endpoint, request.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve SKU available. Status code: {code}", response.StatusCode);
                return new ResultModel<PageData<SkuAvailableDTO>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Request failed with status code: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<PageData<SkuAvailableDTO>>>(responseContent, jsonOptions);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize SKU available response.");
                return new ResultModel<PageData<SkuAvailableDTO>>
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to deserialize response"
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving SKU available");
            return new ResultModel<PageData<SkuAvailableDTO>>
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get available pallets/locations for outbound with FEFO suggestion
    /// GET /api/stock/available-for-outbound?skuId={id}&requiredQty={qty}
    /// </summary>
    public async Task<ResultModel<List<PalletAvailableDTO>>> GetAvailableForOutboundAsync(int skuId, decimal requiredQty)
    {
        try
        {
            if (skuId <= 0)
            {
                return new ResultModel<List<PalletAvailableDTO>>
                {
                    IsSuccess = false,
                    ErrorMessage = "InvalidSkuId"
                };
            }

            if (requiredQty <= 0)
            {
                return new ResultModel<List<PalletAvailableDTO>>
                {
                    IsSuccess = false,
                    ErrorMessage = "InvalidQuantity"
                };
            }

            var client = CreateClient();
            var endpoint = $"/stock/available-for-outbound?skuId={skuId}&requiredQty={requiredQty}";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve available pallets for outbound. Status code: {code}", response.StatusCode);
                return new ResultModel<List<PalletAvailableDTO>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Request failed with status code: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<PalletAvailableDTO>>>(responseContent, jsonOptions);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize available pallets response.");
                return new ResultModel<List<PalletAvailableDTO>>
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to deserialize response"
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving available pallets for outbound");
            return new ResultModel<List<PalletAvailableDTO>>
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<LocationWithPalletViewModel>> GetLocationStockBySku(GetLocationStockBySkuRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/stock/location-by-sku?warehouseID={request.WarehouseId}&skuID={request.SkuId}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve location stock by SKU. Status code: {code} => {res}", response.StatusCode, response);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<LocationWithPalletViewModel>>>(responseContent, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve location stock by SKU. {result}", result);
                return [];
            }
            return result.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving location stock by SKU");
            return [];
        }
    }

    public async Task<List<GoodSkuLocationInfo>> FilterSkuLocationStock(GetLocationStockBySkuRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/stock/filter-location-by-sku?warehouseID={request.WarehouseId}&skuID={request.SkuId}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to filter-location-by-sku. Status code: {code} => {res}", response.StatusCode, response);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<GoodSkuLocationInfo>>>(responseContent, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to filter-location-by-sku. {result}", result);
                return [];
            }
            return result.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while filter-location-by-sku");
            return [];
        }
    }

    public async Task<List<LocationStockInfoDTO>> GetLocationStockInfoAsync(string warehouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/stock/get-location-stock-info/{warehouseId}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve location stock info. Status code: {code} => {res}", response.StatusCode, response);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<LocationStockInfoDTO>>>(responseContent, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve location stock info. {result}", result);
                return [];
            }
            return result.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving location stock info for warehouseId: {warehouseId}", warehouseId);
            return [];
        }
    }

    public async Task<bool> UpsertLocationSyncConflictsAsync(List<UpsertLocationSyncConflictRequest> requests)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/upsert-location-sync-conflicts";
            var response = await client.PostAsync(endpoint, requests.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to upsert location sync conflicts. Status code: {code} => {res}", response.StatusCode, response);
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to deserialize upsert location sync conflicts response. {result}", result);
                return false;
            }
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while upserting location sync conflicts");
            return false;
        }
    }

    public async Task<bool> ResolveWcsOnlyInboundAsync(ResolveWcsOnlyInboundRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/resolve-wcs-only-inbound";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to resolve WCS_ONLY inbound. Status code: {code}", response.StatusCode);
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to resolve WCS_ONLY inbound. {result}", result);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resolving WCS_ONLY inbound");
            return false;
        }
    }

    public async Task<bool> ResolvePalletMergeSameLocationAsync(ResolvePalletMergeSameLocationRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/resolve-pallet-merge-same-location";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to resolve pallet merge same location. Status code: {code}", response.StatusCode);
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);
            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to resolve pallet merge same location. {result}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resolving pallet merge same location");
            return false;
        }
    }

    /// <summary>
    /// Handle for resolve WMS_ONLY clear location issue, which is caused by the scenario that WMS has inventory record but WCS doesn't have any record for the same location, then WMS will not allow to put new inventory into this location until the issue is resolved. This API will help to clear the inventory record in WMS and make the location available for new inventory.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<bool> ResolveWmsOnlyClearLocationAsync(ResolveWmsOnlyClearLocationRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/stock/resolve-wms-only-clear-location";
            var response = await client.PostAsync(endpoint, request.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to resolve WMS_ONLY clear location. Status code: {code}", response.StatusCode);
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);

            return result != null && result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resolving WMS_ONLY clear location");
            return false;
        }
    }

    public async Task<bool> ResolveLocationMismatchAsync(ResolveLocationMismatchRequest request)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync("/stock/resolve-location-mismatch", request.ContentPretty());
            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(content, jsonOptions);
            return result != null && result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resolving location mismatch");
            return false;
        }
    }

    public async Task<List<LocationSyncLogItemDto>> GetLocationSyncLogsAsync(int warehouseId)
    {
        try
        {
            if (warehouseId <= 0) return [];

            var client = CreateClient();
            var endpoint = $"/locations/location-sync-logs?warehouseId={warehouseId}";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get location sync logs. Status code: {code}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<LocationSyncLogItemDto>>>(content, jsonOptions);

            return result != null && result.IsSuccess ? (result.Data ?? []) : [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting location sync logs");
            return [];
        }
    }

    public async Task<List<LocationSyncConflictKeyDto>> GetLocationSyncConflictsByTraceIdAsync(string traceId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(traceId)) return [];

            var client = CreateClient();
            var endpoint = $"/locations/location-sync-conflicts/{Uri.EscapeDataString(traceId)}";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get location sync conflicts by traceId. Status code: {code}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<LocationSyncConflictKeyDto>>>(content, jsonOptions);

            return result != null && result.IsSuccess ? (result.Data ?? []) : [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting location sync conflicts by traceId");
            return [];
        }
    }

    public async Task<(bool success, int canceledLogs, int deletedConflicts, string message)>
        CancelPreviousLogsAndDeleteConflictsAsync(int warehouseId, string traceId)
    {
        try
        {
            if (warehouseId <= 0 || string.IsNullOrWhiteSpace(traceId))
                return (false, 0, 0, "Invalid warehouseId/traceId.");

            var client = CreateClient();
            var endpoint = $"/locations/location-sync-logs/{Uri.EscapeDataString(traceId)}/cancel-previous?warehouseId={warehouseId}";
            var response = await client.PostAsync(endpoint, content: null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to cancel previous logs. Status code: {code}", response.StatusCode);
                return (false, 0, 0, "Call backend failed.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<CancelPreviousLogsResultDto>>(content, jsonOptions);

            if (result == null || !result.IsSuccess || result.Data == null)
                return (false, 0, 0, "Backend returned invalid data.");

            return (true, result.Data.CanceledLogs, result.Data.DeletedConflicts, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while canceling previous logs and deleting conflicts");
            return (false, 0, 0, "Exception occurred.");
        }
    }
}
