using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Services.Receipt
{
    /// <summary>
    /// mapping n-n Receipt Detail and Inbound
    /// </summary>
    [Table("receipt_inbound_detail_integration")]
    public class ReceiptDetailInboundIntegrationEntity : BaseModel
    {
        /// <summary>
        /// receipt details    
        /// </summary>
        [Column("receipt_detail_id")]
        public required int ReceiptDetailId { get; set; }

        /// <summary>
        /// inbound integration
        /// </summary>
        [Column("inbound_id")]
        public required int InboundId { get; set; }

        /// <summary>
        /// Create date
        /// </summary>
        [Column("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
