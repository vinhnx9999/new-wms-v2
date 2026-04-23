using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;

namespace Wms.Theme.Web.Services.Sku;

public class SkuService(IHttpClientFactory httpClientFactory,
    ILogger<SkuService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), ISkuService
{
    public async Task<int> CreateSkuAsync(SkuCreateRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/sku";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return 0;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return 0;
            }
            return result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", request);
            return 0;
        }
    }

    public async Task<(int? data, string? message)> DeleteSku(int skuId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/sku/{skuId}";
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
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization failed when DeleteSku");
            }
            return (result.Data, "success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while DeleteSku {param}", skuId);
            return (0, ex.Message);
        }
    }

    public async Task<IEnumerable<SkuMaster>> GetMasterData()
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/sku/master-data";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response Master Data SKU is not success");
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<SkuMaster>>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return [];
            }
            return result.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API GetMasterData SKU");
            return [];
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<InputSku> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/sku/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response Import Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", request);
            return (0, "Error occurred while calling API Import Excel is not success");
        }
    }

    public async Task<List<SkuSupplierDTO>> PageSearch(PageSearchRequest pageSearch, int? warehouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/sku/list?warehouseId={warehouseId}";
            var response = await client.PostAsync(endpoint, pageSearch.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<DataResponse<SkuSupplierDTO>>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return [];
            }
            return result.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", pageSearch);
            return [];
        }
    }

    public async Task<List<SkuSupplierDTO>> PageSearchSkuSupplier(int? supplierId, PageSearchRequest pageSearch)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/sku/list/{supplierId}";
            var response = await client.PostAsync(endpoint, pageSearch.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<DataResponse<SkuSupplierDTO>>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return [];
            }
            return result.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", pageSearch);
            return [];
        }
    }

    public async Task<IEnumerable<SkuDTO>> SearchData(PageSearchRequest pageSearch, int? warehouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/sku/search-data";
            var response = await client.PostAsync(endpoint, pageSearch.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response SearchData SKU is not success");
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<DataResponse<SkuDTO>>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return [];
            }
            return result.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API SearchData SKU, Param : {param}", pageSearch);
            return [];
        }
    }

    public async Task<(bool isSucess, string message)> UpdateSkuAsync(int id, UpdateSkuRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/sku/{id}";
            var resposne = await client.PatchAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!resposne.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (false, "Response is not success");
            }
            var responseContent = await resposne.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (false, "Deserialization failed");
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", request);
            return (false, "Error occurred while calling API");
        }
    }
}
