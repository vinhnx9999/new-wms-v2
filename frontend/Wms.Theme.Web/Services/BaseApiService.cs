using System.Text.Json;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Services
{
    public abstract class BaseApiService(IHttpClientFactory httpClientFactory, 
        ILogger logger, IConfiguration configuration)
    {
        protected readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        protected readonly ILogger _logger = logger;
        protected readonly string _baseUrl = configuration["Api:BaseUrl"] 
            ?? throw new Exception("API Base URL is not configured.");

        protected readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new CustomDateTimeConverter()
            }
        };

        protected HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("Auth");
            client.BaseAddress = new Uri(_baseUrl);
            return client;
        }
    }
}