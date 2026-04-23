using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    public class SupplierDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("supplier_name")]
        public string SupplierName { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;
        [JsonPropertyName("email")]

        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("manager")]

        public string Manager { get; set; } = string.Empty;

        [JsonPropertyName("contact_tel")]
        public string ContactTel { get; set; } = string.Empty;

        [JsonPropertyName("creator")]

        public string Creator { get; set; } = string.Empty;

        [JsonPropertyName("created_time")]
        public DateTime CreatedTime { get; set; }
        [JsonPropertyName("last_update_time")]

        public DateTime LastUpdateTime { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; } = true;
        [JsonPropertyName("tenant_id")]

        public long TenantId { get; set; } = 0;

        public string TaxNumber { get; set; } = string.Empty;
    }
}
