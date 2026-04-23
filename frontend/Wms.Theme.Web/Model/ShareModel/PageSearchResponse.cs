using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ShareModel
{
    public class PageSearchResponse<T> where T : class
    {

        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public Data<T>? Data { get; set; }
    }

    public class Data<T>
    {
        [JsonPropertyName("rows")]
        public List<T> Rows { get; set; } = new List<T>();

        [JsonPropertyName("totals")]
        public int Totals { get; set; }
    }

}

