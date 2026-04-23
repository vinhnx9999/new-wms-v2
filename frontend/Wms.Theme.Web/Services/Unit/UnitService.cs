using DocumentFormat.OpenXml.Office2016.Excel;
using System.Text.Json;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Units;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Unit;
public interface IUnitService
{
    Task<(int? data, string? message)> DeleteUnit(int unitId);
    Task<List<UnitDTO>> GetAllUnitsAsync();
    Task<(int? data, string? message)> ImportExcelData(List<InputUnitOfMeasure> inputUnits);
}

public class UnitService(IHttpClientFactory httpClientFactory,
    ILogger<UnitService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IUnitService
{
    public async Task<(int? data, string? message)> DeleteUnit(int unitId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/unitOfMeasure/{unitId}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success Delete unitOfMeasure");
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
            _logger.LogError(ex, "Error occurred while Delete unitOfMeasure {param}", unitId);
            return (0, ex.Message);
        }
    }

    public async Task<List<UnitDTO>> GetAllUnitsAsync()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/unitOfMeasure/all";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch Units. Status Code: {statusCode}", response.StatusCode);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ListApiResponse<UnitDTO>>(content, jsonOptions);
            if (responseData == null || !responseData.IsSuccess)
            {
                _logger.LogError("Failed to deserialize Unit data or API returned an error.");
                return [];
            }
            return responseData?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllUnits has {error}", ex.Message);
            throw;
        }
    }

    public async Task<(int? data, string? message)> ImportExcelData(List<InputUnitOfMeasure> request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/unitOfMeasure/import-excel";
            var response = await client.PostAsync(endpoint, request.ContentPretty(jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response unitOfMeasure Import Excel is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization unitOfMeasure Import Excel is not success"); ;
            }
            return (result.Data, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling API , Param : {param}", request);
            return (0, "Error occurred while calling API unitOfMeasure Import Excel is not success");
        }
    }
}
