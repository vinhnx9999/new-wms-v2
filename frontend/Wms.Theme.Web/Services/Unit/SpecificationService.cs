
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Text.Json;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Units;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Unit;

public interface ISpecificationService
{
    Task<(int? data, string? message)> DeleteSpecification(int specificationId);
    Task<List<SpecificationDTO>> GetAllSpecificationsAsync();
    Task<(int? data, string? message)> ImportExcelData(List<InputSpecification> inputSpecifications);
}


public class SpecificationService(IHttpClientFactory httpClientFactory,
    ILogger<UnitService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), ISpecificationService
{
    public async Task<(int? data, string? message)> DeleteSpecification(int specId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/specification/{specId}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success Delete specification");
                return (0, "Response is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError("API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, result?.ErrorMessage ?? "Deserialization failed");
            }
            return (result.Data, "success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while Delete specification {param}", specId);
            return (0, ex.Message);
        }
    }

    public async Task<List<SpecificationDTO>> GetAllSpecificationsAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/specification/all";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch specifications. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ListApiResponse<SpecificationDTO>>(content, jsonOptions);
            if (responseData == null || !responseData.IsSuccess)
            {
                _logger.LogError("Failed to deserialize specification data or API returned an error.");
                return [];
            }
            return responseData?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllSpecifications has {error}", ex.Message);
            throw;
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<InputSpecification> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/specification/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response specifications Import Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Import Excel is not success");
                return (0, result?.ErrorMessage ?? "Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API specifications, Param : {param}", request);
            return (0, "Error occurred while calling API Import Excel is not success");
        }
    }
}
