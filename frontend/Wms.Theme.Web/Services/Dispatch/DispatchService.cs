using System.Text;
using System.Text.Json;
using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.Dispatch
{
    public class DispatchService : BaseApiService, IDispatchService
    {
        public DispatchService(IHttpClientFactory httpClientFactory, ILogger<DispatchService> logger, IConfiguration configuration) : base(httpClientFactory, logger, configuration)
        {
        }

        public async Task<string> GetNextDispatchNoAsync()
        {
            // Generate dispatch number with format: DN + yyyyMMdd + sequence
            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            var sequence = DateTime.Now.ToString("HHmmss");
            return await Task.FromResult($"DN{dateStr}{sequence}");
        }

        public async Task<ApiResponse<DispatchAdvancedDTO>> GetDispatchAdvancedList(PageSearchRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/advanced-list";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch dispatch list. Status Code: " + response.StatusCode);
                    return new ApiResponse<DispatchAdvancedDTO>();
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var dispatchResponse = JsonSerializer.Deserialize<ApiResponse<DispatchAdvancedDTO>>(responseContent);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogError("Dispatch response or data is null");
                    return new ApiResponse<DispatchAdvancedDTO>();
                }
                _logger.LogInformation("Successfully fetched dispatch list.");
                return dispatchResponse ?? new ApiResponse<DispatchAdvancedDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at GetAllDisPatch" + ex.Message);
                return new ApiResponse<DispatchAdvancedDTO>();
            }
        }

        public async Task<bool> AddNewDispatchList(List<DispatchListAddViewModel> request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to add new dispatch list. Status Code: " + response.StatusCode);
                    return false;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var dispatchResponse = JsonSerializer.Deserialize<SimpleApiResponse>(responseContent);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogError("Add Dispatch response is null or unsuccessful");
                    return false;
                }
                _logger.LogInformation("Successfully added new dispatch list.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at AddNewDispatchList" + ex.Message);
                return true;
            }
        }

        public async Task<List<DispatchDetailDTO>> GetByDispatchlistNo(string dispatch_no)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "dispatchlist/by-dispatch_no?dispatch_no=" + dispatch_no;
                var response = await client.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch dispatch detail. Status Code: " + response.StatusCode);
                    return new List<DispatchDetailDTO>();
                }
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var dispatchResponse = JsonSerializer.Deserialize<ListApiResponse<DispatchDetailDTO>>(responseContent, jsonOptions);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogError("Dispatch detail response or data is null");
                    return new List<DispatchDetailDTO>();
                }
                return dispatchResponse?.Data ?? new List<DispatchDetailDTO>();
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Exception at GetByDispatchlistNo" + ex.Message);
                return new List<DispatchDetailDTO>();
            }

        }

        public async Task<bool> RemoveDispatchList(string dispatch_no)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist?dispatch_no=" + dispatch_no;
                var response = await client.DeleteAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to remove dispatch list. Status Code: " + response.StatusCode);
                    return false;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var dispatchResponse = JsonSerializer.Deserialize<SimpleApiResponse>(responseContent);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogError("Remove Dispatch response is null or unsuccessful");
                    return false;
                }
                _logger.LogInformation("Successfully removed dispatch list.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at RemoveDispatchList" + ex.Message);
                return false;
            }
        }

        public async Task<List<DispatchlistConfirmDetailViewModel>> GetDispatchByConfirmCheck(string dispatch_no)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/confirm-check?dispatch_no=" + dispatch_no;
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch dispatch confirm details. Status Code: " + response.StatusCode);
                    return new List<DispatchlistConfirmDetailViewModel>();
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };

                var dispatchResponse = JsonSerializer.Deserialize<ListApiResponse<DispatchlistConfirmDetailViewModel>>(responseContent, jsonOptions);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogError("Dispatch confirm response is null or unsuccessful");
                    return new List<DispatchlistConfirmDetailViewModel>();
                }
                return dispatchResponse.Data ?? new List<DispatchlistConfirmDetailViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at GetDispatchByConfirmCheck" + ex.Message);
                return new List<DispatchlistConfirmDetailViewModel>();
            }
        }

        /// <summary>
        ///  Confirm orders and create  dispatchpicklist
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> ConfirmDispatchHasOrdered(List<DispatchlistConfirmDetailViewModel> request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "dispatchlist/confirm-order";
                var jsonContent = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Response is not valid");
                    return false;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var dispatchResponse = JsonSerializer.Deserialize<SimpleApiResponse>(responseContent);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogWarning("Dispatch confirm response is null or unsuccessful");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at ConfirmDispatchHasOrdered" + ex.Message);
                return false;
            }
        }

        public async Task<bool> ConFirmDispatchHasPicked(string dispatch_no)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/confirm-pick-dispatchlistno?dispatch_no=" + dispatch_no;
                var content = new StringContent("", Encoding.UTF8, "application/json");
                var response = await client.PutAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to confirm dispatch pick. Status Code: " + response.StatusCode);
                    return false;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var dispatchResponse = JsonSerializer.Deserialize<SimpleApiResponse>(responseContent);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogError("Confirm dispatch pick response is null or unsuccessful");
                    return false;
                }

                _logger.LogInformation("Successfully confirmed dispatch pick.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at ConFirmDispatchHasPicked" + ex.Message);
                return false;
            }
        }

        public async Task<bool> CancelOrder(CancelOrderDTO cancelOrderDTO)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/cancel-order";
                var jsonContent = new StringContent(JsonSerializer.Serialize(cancelOrderDTO, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Response is not valid");
                    return false;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var dispatchResponse = JsonSerializer.Deserialize<SimpleApiResponse>(responseContent);
                if (dispatchResponse == null || !dispatchResponse.IsSuccess)
                {
                    _logger.LogWarning("Cancel order response is null or unsuccessful");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at CancelOrder" + ex.Message);
                return false;
            }
        }

        public async Task<ApiResponse<DispatchListDTO>> GetDispatchList(PageSearchRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "dispatchlist/list";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get dispatch list. Status Code: " + response.StatusCode);
                    return null;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var result = JsonSerializer.Deserialize<ApiResponse<DispatchListDTO>>(responseContent, jsonOptions);
                if (result == null || !result.IsSuccess)
                {
                    _logger.LogError("Get dispatch list response is null or unsuccessful");
                    return new ApiResponse<DispatchListDTO>();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Get dispatch list response is null or unsuccessful" + ex.Message);
                return new ApiResponse<DispatchListDTO>();
            }
        }

        public async Task<bool> ConFirmDispatchHasPackage(List<DispatchListPackageDTO> request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/package";
                var content = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> WeighDispatchList(List<DispatchListWeightDTO> request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "dispatchlist/weight";
                var content = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("response is not success");
                    return false;
                }
                var resultContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                };
                var weightResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultContent, jsonOptions);
                if (weightResponse == null || !weightResponse.IsSuccess)
                {
                    _logger.LogError("The response is not success");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("exception" + ex.Message);
                return false;
            }
        }

        public async Task<bool> ConfirmDispatchHasDeliveried(List<DispatchListDeliveryDTO> request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/delivery";
                var content = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("response is not success");
                    return false;
                }
                var resultContent = await response.Content.ReadAsStringAsync();
                var responseResult = JsonSerializer.Deserialize<SimpleApiResponse>(resultContent);
                if (responseResult == null || !responseResult.IsSuccess)
                {
                    _logger.LogError("The response is not success");
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception" + ex.Message);
                return false;
            }
        }

        public async Task<bool> SignDispatchList(List<DispatchListSignDTO> request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/sign";
                var content = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to sign dispatch list");
                    return false;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var signResponse = JsonSerializer.Deserialize<SimpleApiResponse>(responseContent);
                if (signResponse == null || !signResponse.IsSuccess)
                {
                    _logger.LogError("Sign dispatch list response is null or unsuccessful");
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at SignDispatchList" + ex.Message);
                return false;

            }
        }

        #region Duy Phat Solution

        /// <summary>
        /// Create draft dispatch order (status = Draft, no stock lock)
        /// POST /api/dispatchlist/draft
        /// </summary>
        public async Task<DispatchDraftResponse> CreateDraftAsync(DispatchDraftRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/draft";
                var jsonContent = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to create draft dispatch. Status Code: {code}, Response: {response}",
                        response.StatusCode, responseContent);
                    return new DispatchDraftResponse
                    {
                        Code = (int)response.StatusCode,
                        Message = $"Request failed with status code: {response.StatusCode}"
                    };
                }

                var result = JsonSerializer.Deserialize<DispatchDraftResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogError("Failed to deserialize draft dispatch response.");
                    return new DispatchDraftResponse
                    {
                        Code = 500,
                        Message = "Failed to deserialize response"
                    };
                }

                _logger.LogInformation("Successfully created draft dispatch with ID: {id}", result.DispatchId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception at CreateDraftAsync");
                return new DispatchDraftResponse
                {
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Execute draft dispatch (change status to Processing, lock stock)
        /// PUT /api/dispatchlist/execute/{id}
        /// </summary>
        public async Task<DispatchExecuteResponse> ExecuteDraftAsync(int dispatchId)
        {
            try
            {
                if (dispatchId <= 0)
                {
                    return new DispatchExecuteResponse
                    {
                        Code = 400,
                        Message = "Invalid dispatch ID"
                    };
                }

                var client = CreateClient();
                var endpoint = $"/dispatchlist/execute/{dispatchId}";
                var content = new StringContent("", Encoding.UTF8, "application/json");

                var response = await client.PutAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to execute draft dispatch. Status Code: {code}, Response: {response}",
                        response.StatusCode, responseContent);
                    return new DispatchExecuteResponse
                    {
                        Code = (int)response.StatusCode,
                        Message = $"Request failed with status code: {response.StatusCode}"
                    };
                }

                var result = JsonSerializer.Deserialize<DispatchExecuteResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogError("Failed to deserialize execute dispatch response.");
                    return new DispatchExecuteResponse
                    {
                        Code = 500,
                        Message = "Failed to deserialize response"
                    };
                }

                _logger.LogInformation("Successfully executed draft dispatch with ID: {id}", dispatchId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception at ExecuteDraftAsync");
                return new DispatchExecuteResponse
                {
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Create and execute dispatch in one step (skip review)
        /// POST /api/dispatchlist/create-and-execute
        /// </summary>
        public async Task<DispatchDraftResponse> CreateAndExecuteAsync(DispatchDraftRequest request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "/dispatchlist/create-and-execute";
                var jsonContent = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to create and execute dispatch. Status Code: {code}, Response: {response}",
                        response.StatusCode, responseContent);
                    return new DispatchDraftResponse
                    {
                        Code = (int)response.StatusCode,
                        Message = $"Request failed with status code: {response.StatusCode}"
                    };
                }

                var result = JsonSerializer.Deserialize<DispatchDraftResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogError("Failed to deserialize create and execute dispatch response.");
                    return new DispatchDraftResponse
                    {
                        Code = 500,
                        Message = "Failed to deserialize response"
                    };
                }

                _logger.LogInformation("Successfully created and executed dispatch with ID: {id}", result.DispatchId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception at CreateAndExecuteAsync");
                return new DispatchDraftResponse
                {
                    Code = 500,
                    Message = ex.Message
                };
            }
        }

        #endregion duy phat solution
    }
}
