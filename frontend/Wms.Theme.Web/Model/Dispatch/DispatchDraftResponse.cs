using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Dispatch
{
    /// <summary>
    /// Response model for dispatch draft/execute operations
    /// </summary>
    public class DispatchDraftResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public int? DispatchId { get; set; }

        public bool IsSuccess => Code == 200;
    }

    /// <summary>
    /// Response model for execute dispatch
    /// </summary>
    public class DispatchExecuteResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;

        public bool IsSuccess => Code == 200;
    }
}
