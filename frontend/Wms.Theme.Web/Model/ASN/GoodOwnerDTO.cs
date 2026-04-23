using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.ASN
{
    public class GoodOwnerDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;
        [JsonPropertyName("goods_owner_name")]
        public string GoodsOwnerName { get; set; } = string.Empty;
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;
        [JsonPropertyName("manager")]
        public string Manager { get; set; } = string.Empty;
        [JsonPropertyName("contact_tel")]
        public string ContactTel { get; set; } = string.Empty;
        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("last_update_time")]
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; } = true;
    }
}
