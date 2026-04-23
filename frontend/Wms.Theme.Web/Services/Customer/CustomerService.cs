using System.Text.Json;
using Wms.Theme.Web.Model.Customer;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Customer;

public class CustomerService(IHttpClientFactory httpClientFactory,
    ILogger<CustomerService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), ICustomerService
{

    public async Task<List<CustomerDTO>> GetAllCustomersAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("customer/all");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch customers. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var valueContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ListApiResponse<CustomerDTO>>(valueContent, jsonOptions);

            if (responseData == null || !responseData.IsSuccess)
            {
                _logger.LogError("Response data is null when fetching customers.");
                return [];
            }
            return responseData?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching customers.");
            return [];
        }
    }

    public async Task<List<CustomerResponseViewModel>> PageSearchAsync(PageSearchRequest pageSearch)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/customer/list";
            var response = await client.PostAsync(endpoint, pageSearch.ContentPretty());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to page search customer. Status Code: {statusCode} => {res}", response.StatusCode, response);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<DataResponse<CustomerResponseViewModel>>>(content, jsonOptions);
            return result?.Data?.Rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PageSearchCustomer");
            return [];
        }
    }

    public async Task<(int? Id, string? Message)> AddAsync(AddCustomerRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/customer";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to add customer. Status Code: {statusCode} => {res}", response.StatusCode, response);
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

    public async Task<(int? data, string? message)> ImportExcelData(List<InputCustomer> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/customer/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response customer Import Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {endpoint} returned error: {error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization customer Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", request);
            return (0, "Error occurred while calling API customer Import Excel is not success");
        }
    }

    public async Task<(int? data, string? message)> DeleteCustomer(int customerId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/customer/{customerId}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success Delete customer");
                return (0, "Response is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError("API {endpoint} returned error: {error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization failed when Delete customer");
            }
            return (result.Data, "success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while Delete customer {param}", customerId);
            return (0, ex.Message);
        }
    }

    public async Task<(bool isSuccess, string? message)> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/customer/{id}";
            var response = await client.PatchAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to update customer. Status Code: {statusCode} => {res}", response.StatusCode, response);
                return (false, "Failed to update customer");
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<bool>>(content, jsonOptions);
            if (result is null)
            {
                _logger.LogError("Deserialization failed when updating customer. Content: {content}", content);
                return (false, "Deserialization failed when updating customer");
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating customer {param}", request);
            return (false, "Error occurred while updating customer");
        }
    }
}