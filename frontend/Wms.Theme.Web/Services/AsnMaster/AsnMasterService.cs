using System.Text;
using System.Text.Json;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.AsnMaster;

public class AsnMasterService(IHttpClientFactory httpClientFactory, ILogger<AsnMasterService> logger, IConfiguration configuration) : BaseApiService(httpClientFactory, logger, configuration), IAsnMasterService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new CustomDateTimeConverter() }
    };

    public async Task<AsnMasterCreateResponse?> CreateAsnMasterAsync(AsnMasterCustomDetailedDTO request)
    {
        try
        {
            var client = CreateClient();
            var apiEndpoint = "asn/asnmaster";
            var response = await client.PostAsync(apiEndpoint, request.ContentPretty(_jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create ASN Master. Status Code: {StatusCode}", response.StatusCode);
                return new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}" };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            // Log the raw response to see what the API actually returns
            _logger.LogInformation("ASN Master creation API response: {ResponseContent}", responseContent);

            var apiResponse = JsonSerializer.Deserialize<AsnMasterCreateResponse>(responseContent, _jsonOptions);
            if (apiResponse?.IsSuccess == true)
            {
                _logger.LogInformation("Successfully created ASN Master with ID: {CreatedId}", apiResponse.CreatedId);
                return apiResponse;
            }
            else
            {
                _logger.LogError("API returned error while creating ASN Master: {ErrorMessage}", apiResponse?.ErrorMessage ?? "Unknown error");
                return apiResponse ?? new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = "Unknown error" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateAsnMasterAsync");
            return new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AsnMasterCreateResponse?> CreateDraftAsync(AsnMasterCustomDetailedDTO request)
    {
        try
        {
            var client = CreateClient();
            var apiEndpoint = "asn/asnmaster/draft";
            var response = await client.PostAsync(apiEndpoint, request.ContentPretty(_jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create draft ASN. Status Code: {StatusCode}", response.StatusCode);
                return new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}" };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AsnMasterCreateResponse>(responseContent, _jsonOptions);
            return apiResponse ?? new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = "Unknown error" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateDraftAsync");
            return new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AsnMasterCreateResponse?> SubmitAsync(AsnMasterCustomDetailedDTO request)
    {
        try
        {
            var client = CreateClient();
            var apiEndpoint = "asn/asnmaster/submit";
            var response = await client.PostAsync(apiEndpoint, request.ContentPretty(_jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to submit ASN. Status Code: {StatusCode}", response.StatusCode);
                return new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}" };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AsnMasterCreateResponse>(responseContent, _jsonOptions);
            return apiResponse ?? new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = "Unknown error" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubmitAsync");
            return new AsnMasterCreateResponse { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<string> GetNextAsnNo()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "asn/asnmaster/next-asn-no";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get next ASN No. Status Code: {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.IsSuccess == true && !string.IsNullOrWhiteSpace(result.Data))
            {
                return result.Data;
            }

            _logger.LogWarning("GetNextAsnNo API returned unsuccessful response: {ErrorMessage}", result?.ErrorMessage);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNextAsnNo");
            return string.Empty;
        }
    }

    public async Task<AsnMasterApiResponse?> GetAsnMasterListAsync(ListPageModelRequest request)
    {
        try
        {
            var client = CreateClient();
            var apiEndpoint = "asn/asnmaster/list";
            var response = await client.PostAsync(apiEndpoint, request.ContentPretty(_jsonOptions));
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AsnMasterApiResponse>(responseContent, _jsonOptions);

            if (response.IsSuccessStatusCode && apiResponse?.IsSuccess == true)
            {
                _logger.LogInformation("Successfully retrieved {Count} ASN Master records",
                    apiResponse.Data?.Rows?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("API returned error: {ErrorMessage}. Status: {StatusCode}",
                    apiResponse?.ErrorMessage ?? "Unknown error", response.StatusCode);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAsnMasterListAsync");
            return null;
        }
    }

    public async Task<AsnMasterCustomDetailedDTO?> GetAnsMasterDetailed(int id)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "asn/asnmaster?id=" + id;

            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get ASN Master details for ID {Id}. Status Code: {StatusCode}", id, response.StatusCode);
                return new AsnMasterCustomDetailedDTO();
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            // Log the raw response to see what the API actually returns
            _logger.LogInformation("ASN Master detail API response: {ResponseContent}", responseContent);

            var apiResponse = JsonSerializer.Deserialize<AsnMasterDetailResponse>(responseContent, _jsonOptions);
            if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
            {
                _logger.LogInformation("Successfully retrieved ASN Master details for ID: {Id}", id);
                return apiResponse.Data;
            }
            else
            {
                _logger.LogError("API returned error while getting ASN Master details: {ErrorMessage}", apiResponse?.ErrorMessage ?? "Unknown error");
                return new AsnMasterCustomDetailedDTO();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAnsMasterDetailed for ID {Id}", id);
            return new AsnMasterCustomDetailedDTO();
        }
    }

    public async Task<bool> UpdateAnsMaster(AsnMasterCustomDetailedDTO request)
    {
        try
        {
            var client = CreateClient();
            var apiEndpoint = "asn/asnmaster";
            var response = await client.PutAsync(apiEndpoint, request.ContentPretty(_jsonOptions));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to update");
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            //var jsonOptions = new JsonOptions(new CustomDateTimeConverter());
            var data = JsonSerializer.Deserialize<SimpleApiResponse>(responseContent, _jsonOptions);
            if (data == null || !data.IsSuccess)
            {
                _logger.LogError("Failed to Update");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{error}", ex.Message);
            return false;
        }
    }

    public async Task<ResultModel<string>?> CompleteAsnMasterAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ResultModel<string> { IsSuccess = false, ErrorMessage = "Invalid id", Data = "" };
            }

            var client = CreateClient();
            var endpoint = $"asn/asnmaster/complete?id={id}";

            using var body = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PutAsync(endpoint, body);
            var responseContent = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse != null) return apiResponse;

            return new ResultModel<string>
            {
                IsSuccess = response.IsSuccessStatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? "" : $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                Data = ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CompleteAsnMasterAsync for id={Id}", id);
            return new ResultModel<string> { IsSuccess = false, ErrorMessage = ex.Message, Data = "" };
        }
    }

    public async Task<ResultModel<string>?> DeleteAsnMasterAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ResultModel<string> { IsSuccess = false, ErrorMessage = "Invalid id" };
            }

            var client = CreateClient();
            var endpoint = $"asn/asnmaster?id={id}";
            var response = await client.DeleteAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Backend returns ResultModel<string> (even for errors in many cases)
            var apiResponse = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse != null) return apiResponse;

            // Fallback if response isn't parseable
            return new ResultModel<string>
            {
                IsSuccess = response.IsSuccessStatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? "" : $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                Data = ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteAsnMasterAsync for id={Id}", id);
            return new ResultModel<string> { IsSuccess = false, ErrorMessage = ex.Message, Data = "" };
        }
    }

    public async Task<ResultModel<string>?> RetryInboundItemAsync(RetryInboundItemRequest request)
    {
        try
        {
            var client = CreateClient();
            var apiEndpoint = "asn/submit-inbound-retry";
            var jsonContent = JsonSerializer.Serialize(request);
            _logger.LogInformation(jsonContent);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiEndpoint, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retry inbound item. Status Code: {StatusCode}", response.StatusCode);
                return new ResultModel<string> { IsSuccess = false, ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}", Data = "" };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse != null) return apiResponse;

            return new ResultModel<string>
            {
                IsSuccess = response.IsSuccessStatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? "" : $"HTTP {response.StatusCode}: {response.ReasonPhrase}",
                Data = ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RetryInboundItemAsync");
            return new ResultModel<string> { IsSuccess = false, ErrorMessage = ex.Message, Data = "" };
        }
    }
}
