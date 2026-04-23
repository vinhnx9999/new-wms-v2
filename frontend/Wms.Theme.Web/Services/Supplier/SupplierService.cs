using System.Text.Json;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Supplier;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Supplier;

public class SupplierService(IHttpClientFactory httpClientFactory,
    ILogger<SupplierService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), ISupplierService
{
    public async Task<int> AddSuplierAsync(AddSupplierRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "supplier";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to add supplier. Status Code: {StatusCode}", response.StatusCode);
                return 0;
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(content, jsonOptions);
            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to deserialize add supplier response or API returned an error.");
                return 0;
            }
            return result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a supplier.");
            return 0;
        }
    }

    public async Task<(int? data, string? message)> DeleteSupplier(int supplierId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/supplier/{supplierId}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success Delete Supplier");
                return (0, "Response is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError("API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization failed when Delete Supplier");
            }
            return (result.Data, "success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while Delete Supplier {param}", supplierId);
            return (0, ex.Message);
        }
    }

    public async Task<List<SupplierDTO>> GetAllSuppliers()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/supplier/all";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch suppliers. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ListApiResponse<SupplierDTO>>(content, jsonOptions);
            if (responseData == null || !responseData.IsSuccess)
            {
                _logger.LogError("Failed to deserialize supplier data or API returned an error.");
                return [];
            }
            return responseData?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllSuppliers has {error}", ex.Message);
            throw;
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<InputSupplier> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/supplier/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response supplier Import Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization supplier Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", request);
            return (0, "Error occurred while calling API supplier Import Excel is not success");
        }
    }

    public async Task<List<SupplierVM>> PageSearchAsync(PageSearchRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "supplier/list";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch paginated suppliers. Status Code: {StatusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<DataResponse<SupplierVM>>>(content, jsonOptions);
            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to deserialize paginated supplier data or API returned an error.");
                return [];
            }
            return result?.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching paginated suppliers.");
            return [];
        }
    }

    public async Task<(bool isSuccess, string? message)> UpdateSupplierAsync(int id, UpdateSupplierRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"supplier/{id}";
            var response = await client.PatchAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to update supplier. Status Code: {StatusCode}", response.StatusCode);
                return (false, "Failed to update supplier");
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(content, jsonOptions);
            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Failed to deserialize update supplier response or API returned an error.");
                return (false, "Failed to update supplier");
            }
            return (true, "Supplier updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating supplier with ID {SupplierId}.", id);
            return (false, "An error occurred while updating supplier");
        }
    }
}
