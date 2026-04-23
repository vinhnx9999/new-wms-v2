using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Services.Receipt
{
    /// <summary>
    /// mapping n-n Receipt Detail and Outbound integration
    /// </summary>
    [Table("receipt_outbound_detail_integration")]
    public class ReceiptDetailOutboundIntegrationEntity : BaseModel
    {
        /// <summary>
        /// receipt details  
        /// </summary>
        [Column("receipt_detail_id")]
        public required int ReceiptDetailId { get; set; }

        /// <summary>
        /// outbound integration
        /// </summary>
        [Column("outbound_id")]
        public required int OutboundId { get; set; }

        /// <summary>
        /// Create date
        /// </summary>
        [Column("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
