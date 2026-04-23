
using Wms.Theme.Web.Model.Category;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services.Category;

public class CategoryService(IHttpClientFactory httpClientFactory, ILogger<CategoryService> logger, IConfiguration configuration) : BaseApiService(httpClientFactory, logger, configuration), ICategoryService
{
    public async Task<List<CategoryViewModel>> GetAllCategory()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/category/all";
            var response = await client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var categories = System.Text.Json.JsonSerializer.Deserialize<ResultModel<List<CategoryViewModel>>>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new CustomDateTimeConverter() }
                });
                return categories?.Data ?? [];
            }
            else
            {
                _logger.LogError("Failed to fetch categories. Status Code: {StatusCode}", response.StatusCode);
                return [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching categories.");
            return [];
        }
    }
}
