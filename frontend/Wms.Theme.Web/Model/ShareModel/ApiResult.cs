using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ShareModel
{
    public class ApiResult<T>
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
