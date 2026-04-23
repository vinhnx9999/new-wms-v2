using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    /// <summary>
    /// API response specifically for QR Code endpoints that return direct array in data field
    /// </summary>
    public class QrCodeApiResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<GetAsnQrCodeRequest>? Data { get; set; }
    }
}