using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.Customer
{
    public class CustomerDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;


        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; } = string.Empty;


        [JsonPropertyName("city")]

        public string City { get; set; } = string.Empty;


        [JsonPropertyName("address")]
        public string address { get; set; } = string.Empty;
        [JsonPropertyName("manager")]

        public string manager { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("contact_tel")]
        public string ContactTel { get; set; } = string.Empty;

        [JsonPropertyName("creator")]

        public string Creator { get; set; } = string.Empty;


        [JsonPropertyName("create_time")]

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;


        [JsonPropertyName("last_update_time")]

        public DateTime last_update_time { get; set; } = DateTime.UtcNow;


        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; } = true;

        [JsonPropertyName("tax_number")]
        public string TaxNumber { get; set; } = string.Empty;
    }
}
