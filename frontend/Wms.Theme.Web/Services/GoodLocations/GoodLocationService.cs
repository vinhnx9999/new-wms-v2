using System.Text.Json;
using Wms.Theme.Web.Model.GoodLocation;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Warehouse;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.GoodLocations;

public class GoodLocationService(IHttpClientFactory httpClientFactory,
    ILogger<GoodLocationService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IGoodLocationService
{

    public async Task<List<GoodLocationDto>> GetGoodsLocationPageAsync(PageSearchRequest pageSearchRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "goodslocation/list";
            _logger.LogInformation(" Calling API endpoint: {endpoint} with request: {request}", endpoint, JsonSerializer.Serialize(pageSearchRequest));

            var content = new StringContent(JsonSerializer.Serialize(pageSearchRequest), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching good locations: {response.StatusCode}", response.StatusCode);
                return [];
            }

            return await DeserializeGoodLocationResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching good locations");
            return [];
        }
    }

    public async Task<ResultModel<PageData<GoodLocationDto>>> GetGoodLocationPageAsyncWithFormat(PageSearchRequest pageSearchRequest)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "goodslocation/list";
            var response = await client.PostAsync(endpoint, pageSearchRequest.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API call failed with status code: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ResultModel<PageData<GoodLocationDto>>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateTimeConverter() }
            });
            if (apiResponse == null || !apiResponse.IsSuccess)
            {
                throw new Exception("Failed to deserialize API response or API returned an error.");
            }
            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching good locations");
            return new ResultModel<PageData<GoodLocationDto>>();
        }
    }

    public async Task<List<GoodLocationDto>> GetAvailableLocationsForPalletAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "goodslocation/available-for-pallet";
            _logger.LogInformation("Calling API endpoint: {endpoint}", endpoint);

            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching available locations for pallet: {statusCode}", response.StatusCode);
                return [];
            }
            return await DeserializeGoodLocationResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available locations for pallet");
            return [];
        }
    }

    public async Task<List<GoodLocationDto>> GetAvailableLocationsForPalletAsync(int totalPalletNeed)
    {
        try
        {
            var client = CreateClient();
            // Backend supports default type=Inbound; we only pass totalPalletNeed.
            var endpoint = $"goodslocation/available-for-pallet?totalPalletNeed={totalPalletNeed}";
            _logger.LogInformation("Calling API endpoint: {endpoint}", endpoint);

            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching available locations for pallet: {statusCode}", response.StatusCode);
                return [];
            }
            return await DeserializeGoodLocationResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available locations for pallet");
            return [];
        }
    }
    private async Task<List<GoodLocationDto>> DeserializeGoodLocationResponseAsync(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ResultModel<PageData<GoodLocationDto>>>(responseContent, jsonOptions);

        var result = apiResponse?.Data.Rows; ;
        _logger.LogInformation(" Deserialized {result.Count} locations. First location sample: {result}", result.Count, (result.Count != 0 ? JsonSerializer.Serialize(result.First()) : "None"));

        return result;
    }

    public async Task<IEnumerable<StoreLocationDto>> GetStoreLocationsAsync(int warehouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"goodslocation/available-store-locations/{warehouseId}";
            _logger.LogInformation("Calling API endpoint: {endpoint}", endpoint);

            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching available locations for pallet: {statusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<StoreLocationDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available locations for pallet");
            return [];
        }
    }

    public async Task<List<LocationWithPalletViewModel>> GetGoodLocationWithPalletAsync(GetLocationWithPalletRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "goodslocation/list-location-with-pallet";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API call failed with status code: {statusCode}", response.StatusCode);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ListApiResponse<LocationWithPalletViewModel>>(responseContent, jsonOptions);
            if (apiResponse == null || !apiResponse.IsSuccess)
            {
                _logger.LogError("Failed to deserialize API response or API returned an error.");
                return [];
            }
            return apiResponse.Data ?? [];

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching good location with pallet");
            return [];
        }
    }

    public async Task<List<LocationWithPalletViewModel>> SearchLocationWithSkuAsync(GetLocationWithSkuIdRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "goodslocation/list-location-available-sku";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API call failed with status code: {statusCode}", response.StatusCode);
                return [];
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ListApiResponse<LocationWithPalletViewModel>>(responseContent, jsonOptions);
            if (apiResponse == null || !apiResponse.IsSuccess)
            {
                _logger.LogError("Failed to deserialize API response or API returned an error.");
                return [];
            }
            return apiResponse.Data ?? [];

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching good location with pallet");
            return [];
        }
    }

    public async Task<IEnumerable<WcsBlockLocationDto>> GetWcsBlocksAsnyc()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/locations/wcs-blocks";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get list from WCS blocks. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<WcsBlockLocationDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetWcsBlocksAsnyc");
            return [];
        }
    }

    public async Task<List<PalletLocactionDTO>> GetWcsPalletlocationAsync(string blockId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/locations/pallet/{blockId}";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get pallet locations for block {blockId}. Status Code: {statusCode}", blockId, response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<PalletLocactionDTO>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pallet locations for block {blockId}", blockId);
            return [];
        }
    }

    public async Task<int> CreateLocationAsync(AddLocationRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "goodslocation";

            var response = await client.PostAsync(endpoint, request.ContentPretty());
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Create location failed. Status: {StatusCode}, Response: {Content}", response.StatusCode, content);
                return 0;
            }

            var result = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);
            if (result?.IsSuccess == true && result.Data > 0)
            {
                return result.Data;
            }

            _logger.LogError("Create location failed. Error: {Error}", result?.ErrorMessage ?? "Unknown error");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating location");
            return 0;
        }
    }

    public async Task<IEnumerable<LocationOnlyDto>> GetLocationsByWarehouseAsync(int warehouseId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"goodslocation/locations/{warehouseId}";
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching locations-only: {statusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<List<LocationOnlyDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching locations-only");
            return [];
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<InputLocationExcel> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/goodslocation/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response Import Excel Location is not success");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);

            if (result is null || !result.IsSuccess)
            {
                _logger.LogError("API {Endpoint} returned error: {Error}", endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, result?.ErrorMessage ?? "Import Excel is not success");
            }

            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API Import Excel Location, Param : {param}", request);
            return (0, "Error occurred while calling API Import Excel Location");
        }
    }
}