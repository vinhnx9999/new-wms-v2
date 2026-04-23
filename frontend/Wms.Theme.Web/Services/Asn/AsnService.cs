using System.Globalization;
using System.Text;
using System.Text.Json;
using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.Delivery;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sorted;
using Wms.Theme.Web.Model.Unload;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.Asn
{
    public class AsnService : BaseApiService, IAsnService
    {
        public AsnService(IHttpClientFactory httpClientFactory, ILogger<AsnService> logger, IConfiguration configuration)
            : base(httpClientFactory, logger, configuration)
        {
        }

        public async Task<AsnApiResponse> GetAsnListAsync(PageSearchRequest request)
        {
            try
            {
                var client = CreateClient();

                // Get API endpoint
                var apiEndpoint = "asn/list";

                // Serialize the request to JSON
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Making POST request to ASN API: {Endpoint}", apiEndpoint);

                // Make the POST request
                var response = await client.PostAsync(apiEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the response
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }
                    };

                    var apiResponse = JsonSerializer.Deserialize<AsnApiResponse>(responseContent, options);

                    if (apiResponse?.IsSuccess == true)
                    {
                        _logger.LogInformation("Successfully retrieved {Count} ASN records",
                            apiResponse.Data?.Rows?.Count ?? 0);
                    }
                    else
                    {
                        _logger.LogWarning("API returned error: {ErrorMessage}",
                            apiResponse?.ErrorMessage ?? "Unknown error");
                    }

                    return apiResponse ?? new AsnApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to deserialize API response",
                        Code = 500
                    };
                }
                else
                {
                    _logger.LogError("API request failed. Status: {StatusCode}, Reason: {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);

                    return new AsnApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API request failed with status code: {response.StatusCode}",
                        Code = (int)response.StatusCode
                    };
                }
            }

            catch (TaskCanceledException)
            {
                _logger.LogInformation("Request to {Endpoint} was canceled by client", "asn/list");
                return new AsnApiResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request canceled",
                    Code = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while calling ASN API");
                return new AsnApiResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "An unexpected error occurred.",
                    Code = 500
                };
            }
        }

        public async Task<AsnDetailApiResponse> GetAsnDetailsAsync(int asnId)
        {
            try
            {
                var client = CreateClient();

                // Get API endpoint for ASN details
                var apiEndpoint = $"asn?id={asnId}";

                _logger.LogInformation("Making GET request to ASN Details API: {Endpoint}", apiEndpoint);

                // Make the GET request
                var response = await client.GetAsync(apiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the response
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }
                    };

                    var apiResponse = JsonSerializer.Deserialize<AsnDetailApiResponse>(responseContent, options);

                    if (apiResponse?.IsSuccess == true)
                    {
                        _logger.LogInformation("Successfully retrieved ASN details for ID: {AsnId}", asnId);
                    }
                    else
                    {
                        _logger.LogWarning("API returned error for ASN ID {AsnId}: {ErrorMessage}",
                            asnId, apiResponse?.ErrorMessage ?? "Unknown error");
                    }

                    return apiResponse ?? new AsnDetailApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to deserialize API response",
                        Code = 500
                    };
                }
                else
                {
                    _logger.LogError("ASN Details API request failed. Status: {StatusCode}, Reason: {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);

                    return new AsnDetailApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API request failed with status code: {response.StatusCode}",
                        Code = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while calling ASN Details API for ID: {AsnId}", asnId);
                return new AsnDetailApiResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "An unexpected error occurred.",
                    Code = 500
                };
            }
        }

        public async Task<AsnUpdateResponse> UpdateAsnAsync(AsnUpdateRequest request)
        {
            try
            {
                var client = CreateClient();

                // Get API endpoint for ASN update
                var apiEndpoint = "asn";

                // Serialize the request to JSON
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new CustomDateTimeConverter() }
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Making PUT request to ASN Update API: {Endpoint}", apiEndpoint);

                // Make the PUT request
                var response = await client.PutAsync(apiEndpoint, content);

                // Always try to read and parse the response body first
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log raw response for debugging
                _logger.LogInformation("ASN Update API Response - Status: {StatusCode}, Content-Type: {ContentType}, Raw Content: {ResponseContent}",
                    response.StatusCode, response.Content.Headers.ContentType, responseContent);

                try
                {
                    // Check if response content looks like JSON
                    if (string.IsNullOrWhiteSpace(responseContent) || !responseContent.Trim().StartsWith("{"))
                    {
                        _logger.LogWarning("Response content is not valid JSON format. Content: {ResponseContent}", responseContent);
                        return new AsnUpdateResponse
                        {
                            IsSuccess = false,
                            ErrorMessage = $"Invalid response format from server. Response: {responseContent}",
                            Code = (int)response.StatusCode
                        };
                    }

                    // Log the raw response for debugging
                    _logger.LogInformation("Raw API response for ASN update ID {AsnId}: {ResponseContent}", request.Id, responseContent);

                    // Deserialize the response
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }
                    };

                    var apiResponse = JsonSerializer.Deserialize<AsnUpdateResponse>(responseContent, options);

                    if (apiResponse != null)
                    {
                        // Log based on the API response, not HTTP status
                        if (apiResponse.IsSuccess)
                        {
                            _logger.LogInformation("Successfully updated ASN ID: {AsnId}", request.Id);
                        }
                        else
                        {
                            _logger.LogWarning("API returned error for ASN update ID {AsnId}: {ErrorMessage} (HTTP Status: {StatusCode})",
                                request.Id, apiResponse.ErrorMessage ?? "Unknown error", response.StatusCode);
                        }

                        return apiResponse;
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize API response. Response content: {ResponseContent}", responseContent);
                }

                // Fallback if JSON parsing failed
                _logger.LogError("ASN Update API request failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Response: {ResponseContent}",
                    response.StatusCode, response.ReasonPhrase, responseContent);

                return new AsnUpdateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"API request failed with status code: {response.StatusCode}. Response: {responseContent}",
                    Code = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating ASN ID: {AsnId}", request.Id);
                return new AsnUpdateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "An unexpected error occurred.",
                    Code = 500
                };
            }
        }

        public async Task<AsnCreateResponse> CreateAsnAsync(AsnUpdateRequest request)
        {
            try
            {
                var client = CreateClient();

                // Get API endpoint for ASN creation
                var apiEndpoint = "asn";

                // Serialize the request to JSON
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new CustomDateTimeConverter() }
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Making POST request to ASN Create API: {Endpoint}", apiEndpoint);

                // Make the POST request
                var response = await client.PostAsync(apiEndpoint, content);

                // Always try to read and parse the response body first
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log raw response for debugging
                _logger.LogInformation("ASN Create API Response - Status: {StatusCode}, Content-Type: {ContentType}, Raw Content: {ResponseContent}",
                    response.StatusCode, response.Content.Headers.ContentType, responseContent);

                try
                {
                    // Check if response content looks like JSON
                    if (string.IsNullOrWhiteSpace(responseContent) || !responseContent.Trim().StartsWith("{"))
                    {
                        _logger.LogWarning("Response content is not valid JSON format. Content: {ResponseContent}", responseContent);
                        return new AsnCreateResponse
                        {
                            IsSuccess = false,
                            ErrorMessage = $"Invalid response format from server. Response: {responseContent}",
                            Code = (int)response.StatusCode
                        };
                    }

                    // Deserialize the response
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }
                    };

                    var apiResponse = JsonSerializer.Deserialize<AsnCreateResponse>(responseContent, options);

                    if (apiResponse != null)
                    {
                        // Log based on the API response, not HTTP status
                        if (apiResponse.IsSuccess)
                        {
                            _logger.LogInformation("Successfully created ASN. New ASN ID: {CreatedId}", apiResponse.CreatedId);
                        }
                        else
                        {
                            _logger.LogWarning("API returned error for ASN creation: {ErrorMessage} (HTTP Status: {StatusCode})",
                                apiResponse.ErrorMessage ?? "Unknown error", response.StatusCode);
                        }

                        return apiResponse;
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize API response. Response content: {ResponseContent}", responseContent);
                }

                // Fallback if JSON parsing failed
                _logger.LogError("ASN Create API request failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Response: {ResponseContent}",
                    response.StatusCode, response.ReasonPhrase, responseContent);

                return new AsnCreateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"API request failed with status code: {response.StatusCode}. Response: {responseContent}",
                    Code = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating ASN");
                return new AsnCreateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "An unexpected error occurred.",
                    Code = 500
                };
            }
        }

        public async Task<AsnDeleteResponse> DeleteAsnAsync(int asnId)
        {
            try
            {
                var client = CreateClient();
                // Get API endpoint for ASN deletion
                var apiEndpoint = $"asn/asnmaster?id={asnId}";
                _logger.LogInformation("Making DELETE request to ASN Delete API: {Endpoint}", apiEndpoint);

                // Make the DELETE request
                var response = await client.DeleteAsync(apiEndpoint);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        // Backend returns ResultModel<string>
                        var resultModel = JsonSerializer.Deserialize<ResultModel<string>>(responseContent, options);

                        if (resultModel != null)
                        {
                            if (resultModel.IsSuccess)
                            {
                                _logger.LogInformation("Successfully deleted ASN ID: {AsnId}", asnId);
                                return new AsnDeleteResponse
                                {
                                    IsSuccess = true,
                                    Code = (int)response.StatusCode,
                                    ErrorMessage = resultModel.Data ?? resultModel.ErrorMessage
                                };
                            }
                            else
                            {
                                _logger.LogWarning("Delete logical failure for ASN ID {AsnId}: {Message}", asnId, resultModel.ErrorMessage);
                                return new AsnDeleteResponse
                                {
                                    IsSuccess = false,
                                    Code = 400,
                                    ErrorMessage = !string.IsNullOrEmpty(resultModel.ErrorMessage) ? resultModel.ErrorMessage : "Unknown error"
                                };
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Fallback for simple boolean string or older format
                        if (responseContent.ToLower().Contains("false"))
                        {
                            return new AsnDeleteResponse { IsSuccess = false, Code = 400, ErrorMessage = "Delete failed (Response parse error)" };
                        }
                    }

                    return new AsnDeleteResponse
                    {
                        IsSuccess = true,
                        Code = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogError("ASN Delete API request failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Response: {ResponseContent}",
                        response.StatusCode, response.ReasonPhrase, responseContent);
                    return new AsnDeleteResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API request failed with status code: {response.StatusCode}. Response: {responseContent}",
                        Code = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Delete failed with exception" + ex.Message);
                return new AsnDeleteResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed",
                    Code = 500
                };
            }
        }



        /// <summary>
        /// change the ans_status 0 -> 1 
        /// </summary>
        /// <param name="request"> id , expiry date</param>
        /// <returns></returns>
        public async Task<(bool success, string message)> ConfirmForDeliveryAsnAsync(List<ConfirmModel> request)
        {
            if (request == null || request.Count == 0)
            {
                _logger.LogWarning("ConfirmForDeliveryAsnAsync called with null or empty request.");
                return (false, "Request cannot be null or empty.");
            }
            try
            {
                var requestPayload = request.Select(x => new
                {
                    id = x.Id,
                    arrival_time = DateTime.ParseExact(
                        x.ArrivalTime,
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture
                   ).ToString("yyyy-MM-ddTHH:mm:ss"),
                    input_qty = x.InPutQty,
                }).ToList();

                var client = CreateClient();
                var apiEndpoint = "asn/confirm";
                var jsonContent = JsonSerializer.Serialize(requestPayload);
                _logger.LogInformation("DEBUG JSON SENT: {Json}", jsonContent);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(apiEndpoint, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<SimpleApiResponse>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (data != null && data.IsSuccess)
                    {
                        _logger.LogInformation("Successfully confirmed ASN records.");
                        return (true, data.Data ?? "Success");
                    }

                }
                return (false, "Error confirming ASN records");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming ASN records.");
                return (false, "System error occurred");
            }
        }


        /// <summary>
        /// change the ans_status 1 -> 0
        /// </summary>
        /// <param name="request">id , expiry date </param>
        /// <returns></returns>
        public async Task<bool> RejectForDeliveryAsnAsync(List<int> request)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/confirm-cancel";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(apiEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }

                    });

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        _logger.LogInformation("Successfully confirmed ASN records.");
                        return true;
                    }

                }
                _logger.LogWarning("Failed to confirm ASN records. Status: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming ASN records.");
                return false;
            }
        }


        /// <summary>
        /// change the ans_status 1 -> 2
        /// </summary>
        /// <param name="request"> id , time , personId , person </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<(bool success, string message)> ConfirmForUnLoadAsnAsync(List<UnloadConfirmRequest> request)
        {
            try
            {
                var client = CreateClient();
                var endpoint = "asn/unload";

                var payload = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(endpoint, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<SimpleApiResponse>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }
                    });

                    if (data != null && data.IsSuccess)
                    {
                        _logger.LogInformation("Confirm Success");
                        return (true, "Unload confirmed successfully.");
                    }
                    return (false, data?.ErrorMessage ?? "Failed to confirm unload.");
                }
                try
                {
                    var errorData = JsonSerializer.Deserialize<SimpleApiResponse>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorData?.ErrorMessage ?? $"Failed with status: {response.StatusCode}");
                }
                catch
                {
                    _logger.LogWarning("Failed to confirm unload. Status: {StatusCode}", response.StatusCode);
                    return (false, $"Error: {response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming unload.");
                return (false, "System error occurred");
            }

        }

        /// <summary>
        /// Reject Unload ASN , change the ans_status 2 -> 1
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<(bool success, string message)> RejectUnloadAsync(List<int> request)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/unload-cancel";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(apiEndpoint, content);
                var resultResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }

                    });

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        _logger.LogInformation("Successfully confirmed ASN records.");
                        return (true, "Rejected successfully.");
                    }
                    return (false, apiResponse?.ErrorMessage ?? "Failed to reject.");
                }
                _logger.LogWarning("Failed to confirm ASN records. Status: {StatusCode}", response.StatusCode);

                // Try to parse error message
                try
                {
                    var errorData = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (false, errorData?.ErrorMessage ?? $"Failed with status: {response.StatusCode}");
                }
                catch
                {
                    return (false, $"Error: {response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming ASN records.");
                return (false, "System error occurred");
            }
        }

        /// <summary>
        /// Added ASN to ASN Sorted 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<(bool success, string message)> AddedAsnToAnsSortedAsync(List<AddAnsToAnsSortedRequest> request)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/sorting";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiEndpoint, content);

                var resultResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Failed to added new Asn Sorted");
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, errorData?.ErrorMessage ?? $"Failed with status: {response.StatusCode}");
                    }
                    catch
                    {
                        return (false, $"Error: {response.StatusCode}");
                    }
                }

                var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (apiResponse == null)
                {
                    _logger.LogInformation("Response null");
                    return (false, "Server Didn't Return Value");
                }
                if (!apiResponse.IsSuccess)
                {
                    _logger.LogInformation("Return not success");
                    return (false, apiResponse.ErrorMessage ?? "Failed to add.");
                }
                _logger.LogInformation("Added Asn to Asn Sorted Success");
                return (true, apiResponse.ErrorMessage ?? "Created Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return (false, "System error occurred");
            }
            finally
            {
                _logger.LogInformation("Finished processing AddedAsnToAnsSortedAsync");
            }
        }

        /// <summary>
        /// update quantiy sorted in ASN Sorted
        /// </summary>
        /// <param name="requests"></param>
        /// <returns>bool</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateAsnSortedAsync(List<UpdateAnsSortedRequest> requests)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/sorting-modify";
                var jsonContent = JsonSerializer.Serialize(requests, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Failed to update Asn Sorted");
                    return false;
                }
                var resultResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (apiResponse == null || !apiResponse.IsSuccess)
                {
                    _logger.LogInformation("Response null");
                    return false;
                }
                _logger.LogInformation("Update Asn Sorted Success");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// return list ansSorted by ansId
        /// </summary>
        /// <param name="id"> ansId</param>
        /// <returns>list ansSorted or empty list</returns>
        public async Task<List<AnsSortedResponse>> GetAnsSortedByAnsIDAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/sorting?asn_id=" + id;
                var response = await client.GetAsync(apiEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get ASN Sorted data. Status: {StatusCode}", response.StatusCode);
                    return new List<AnsSortedResponse>();
                }
                var resultResponse = await response.Content.ReadAsStringAsync();

                // Log the raw response for debugging
                _logger.LogInformation("Raw API response for ASN sorted ID {AsnId}: {ResponseContent}", id, resultResponse);

                // Create a custom response model to handle the server's JSON structure
                var apiResponse = JsonSerializer.Deserialize<ListApiResponse<AnsSortedResponse>>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });

                if (apiResponse == null || !apiResponse.IsSuccess)
                {
                    _logger.LogWarning("Response null or not successful for ASN ID: {AsnId}", id);
                    return new List<AnsSortedResponse>();
                }

                _logger.LogInformation("Successfully retrieved {Count} ASN sorted records for ASN ID: {AsnId}",
                    apiResponse.Data?.Count ?? 0, id);
                return apiResponse.Data ?? new List<AnsSortedResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting ASN sorted data for ID: {AsnId}", id);
                return new List<AnsSortedResponse>();
            }
        }

        /// <summary>
        /// confirm Asn Sorted , change status 2 -> 3
        /// </summary>
        /// <param name="ids">list ans id selected</param>
        /// <returns>bool</returns>
        public async Task<(bool success, string message)> ConfirmForSortedAsnAsync(List<int> ids)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/sorted";

                var jsonContent = JsonSerializer.Serialize(ids, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiEndpoint, content);
                var resultResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Try to parse error
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, errorData?.ErrorMessage ?? $"Failed with status: {response.StatusCode}");
                    }
                    catch
                    {
                        return (false, $"Error: {response.StatusCode}");
                    }
                }

                var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });

                if (apiResponse == null || !apiResponse.IsSuccess)
                {
                    return (false, apiResponse?.ErrorMessage ?? "Failed to confirm.");
                }
                return (true, "Confirmed QC successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming sorted.");
                return (false, "System error occurred");
            }
        }


        /// <summary>
        /// change status 3 -> 2
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<(bool success, string message)> RejectForSortedAsnAsync(List<int> ids)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/sorted-cancel";
                var content = new StringContent(JsonSerializer.Serialize(ids, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiEndpoint, content);
                var resultResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Try to parse error
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, errorData?.ErrorMessage ?? $"Failed with status: {response.StatusCode}");
                    }
                    catch
                    {
                        return (false, $"Error: {response.StatusCode}");
                    }
                }

                var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (apiResponse == null || !apiResponse.IsSuccess)
                {
                    _logger.LogInformation("Response null or not success");
                    return (false, apiResponse?.ErrorMessage ?? "Failed to reject status.");
                }
                _logger.LogInformation("Reject Asn Sorted Success");
                return (true, "Putaway rejected successfully.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
                return (false, "System error occurred");
            }
        }

        /// <summary>
        /// get Asn Pending Putaway by asn id
        /// </summary>
        /// <param name="id">asn id</param>
        /// <returns>list of GetAsnPutawayResponse</returns>
        public async Task<List<GetAsnPutawayResponse>> GetAsnPendingPutawayAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/pending-putaway?id=" + id;
                var response = await client.GetAsync(apiEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get ASN Pending Putaway data. Status: {StatusCode}", response.StatusCode);
                    return new List<GetAsnPutawayResponse>();
                }
                var resultResponse = await response.Content.ReadAsStringAsync();

                // Create a wrapper response to handle the API structure
                var wrapperResponse = JsonSerializer.Deserialize<ListApiResponse<GetAsnPutawayResponse>>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });

                if (wrapperResponse == null || !wrapperResponse.IsSuccess)
                {
                    _logger.LogWarning("Response null or not successful when getting ASN Pending Putaway data. Error: {ErrorMessage}",
                        wrapperResponse?.ErrorMessage ?? "Unknown error");
                    return new List<GetAsnPutawayResponse>();
                }

                var apiResponse = wrapperResponse.Data ?? new List<GetAsnPutawayResponse>();
                _logger.LogInformation("Successfully retrieved {Count} ASN Pending Putaway items for ASN ID: {AsnId}",
                    apiResponse.Count, id);
                return apiResponse;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
                return new List<GetAsnPutawayResponse>();

            }

        }

        /// <summary>
        /// update putaway to change ans_status 3 -> 4
        /// </summary>
        /// <param name="requests">list asn Putaway</param>
        /// <returns>bool</returns>
        public async Task<(bool success, int flag, string message)> UpdatePutaway(List<UpdatePutawayRequest> requests)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/putaway";
                var content = new StringContent(JsonSerializer.Serialize(requests, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiEndpoint, content);
                var resultResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Failed to update putaway");
                    // Try to parse error
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, 0, errorData?.ErrorMessage ?? $"Failed to update with status: {response.StatusCode}");
                    }
                    catch
                    {
                        return (false, 0, $"Error: {response.StatusCode}");
                    }
                }

                var apiResponse = JsonSerializer.Deserialize<SimpleApiResponse>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                _logger.LogInformation("Update putaway success");
                if (apiResponse == null || !apiResponse.IsSuccess)
                {
                    _logger.LogInformation("Response null or not success");
                    return (false, 0, apiResponse?.ErrorMessage ?? "Failed to update putaway.");
                }
                var message = apiResponse.Data;
                if (message != null && message.Contains("change_status:True"))
                {
                    _logger.LogInformation("Putaway changed status successfully");
                    return (true, 1, "Putaway thành công! ASN đã được hoàn tất.");
                }
                _logger.LogInformation("Update putaway success");
                return (true, 0, "Putaway thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return (false, 0, "System error occurred");
            }
        }

        public async Task<List<GetAsnQrCodeRequest>> ShowQrCode(List<int> id)
        {
            try
            {
                var client = CreateClient();
                var endPoint = "asn/print-sn";
                var content = new StringContent(JsonSerializer.Serialize(id, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }), Encoding.UTF8, "application/json");

                _logger.LogInformation("Making POST request to QR Code API: {Endpoint} with {Count} ASN IDs", endPoint, id?.Count ?? 0);

                var response = await client.PostAsync(endPoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultResponse = await response.Content.ReadAsStringAsync();

                    // Log raw response for debugging
                    _logger.LogInformation("QR Code API Response - Status: {StatusCode}, Raw Content: {ResponseContent}",
                        response.StatusCode, resultResponse);

                    // Check if response content looks like JSON
                    if (string.IsNullOrWhiteSpace(resultResponse) || !resultResponse.Trim().StartsWith("{"))
                    {
                        _logger.LogWarning("Response content is not valid JSON format. Content: {ResponseContent}", resultResponse);
                        return new List<GetAsnQrCodeRequest>();
                    }

                    // Use the QrCodeApiResponse model
                    var apiResponse = JsonSerializer.Deserialize<QrCodeApiResponse>(resultResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new CustomDateTimeConverter() }
                    });

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        _logger.LogInformation("Successfully generated QR codes for {Count} ASN records", apiResponse.Data?.Count ?? 0);
                        return apiResponse.Data ?? new List<GetAsnQrCodeRequest>();
                    }
                    else
                    {
                        _logger.LogWarning("API returned error for QR code generation: {ErrorMessage}", apiResponse?.ErrorMessage ?? "Unknown error");
                        return new List<GetAsnQrCodeRequest>();
                    }
                }
                else
                {
                    _logger.LogWarning("QR Code API request failed. Status: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                    return new List<GetAsnQrCodeRequest>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating QR codes for ASN IDs: {AsnIds}", string.Join(", ", id ?? new List<int>()));
                return new List<GetAsnQrCodeRequest>();
            }
        }

        public async Task<Dictionary<int, int>> GetTotalRecord()
        {
            try
            {
                var client = CreateClient();
                var endpoint = "asn/status-count";
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get total record counts. Status: {StatusCode}", response.StatusCode);
                    return new Dictionary<int, int>();
                }
                var resultResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResult<Dictionary<int, int>>>(resultResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                if (apiResponse == null || !apiResponse.IsSuccess)
                {
                    _logger.LogWarning("Response null or not successful when getting total record counts. Error: {ErrorMessage}",
                        apiResponse?.ErrorMessage ?? "Unknown error");
                    return new Dictionary<int, int>();
                }
                return apiResponse.Data ?? new Dictionary<int, int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting total record counts");
                return new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// Confirm Robot has Success To putaway
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<(bool success, string message)> ConfirmRobotSuccessAsync(List<int> ids)
        {
            try
            {
                var client = CreateClient();
                var apiEndpoint = "asn/confirm-robot";
                // Serialize the list of IDs
                var jsonContent = JsonSerializer.Serialize(ids);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Use PATCH as per backend definition
                var response = await client.PatchAsync(apiEndpoint, content);
                var resultResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ResultModel<string>>(resultResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        return (true, apiResponse.Data ?? "Success");
                    }
                    return (false, apiResponse?.ErrorMessage ?? "Failed");
                }

                return (false, $"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming robot success.");
                return (false, "System error occurred");
            }
        }
    }
}

