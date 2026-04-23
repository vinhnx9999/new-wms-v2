using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// customer entity
    /// </summary>
    [Table("customer")]
    public class CustomerEntity : BaseModel, ITenantEntity
    {
        #region Property
        /// <summary>
        /// customer name
        /// </summary>
        public string customer_name { get; set; } = string.Empty;

        /// <summary>
        /// city
        /// </summary>
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// address
        /// </summary>
        public string address { get; set; } = string.Empty;

        /// <summary>
        /// email
        /// </summary>
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// manager
        /// </summary>
        public string manager { get; set; } = string.Empty;

        /// <summary>
        /// contact tel
        /// </summary>
        public string contact_tel { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create time
        /// </summary>
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last update time
        /// </summary>
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// valid
        /// </summary>
        public bool is_valid { get; set; } = true;

        /// <summary>
        /// the tenant id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;
        /// <summary>
        /// customer_code
        /// </summary>
        public string? customer_code { get; set; } = "";
        /// <summary>
        /// tax_number
        /// </summary>
        public string? tax_number { get; set; } = "";

        #endregion
    }
}
