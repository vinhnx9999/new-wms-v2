using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Stock
{
    /// <summary>
    /// Stock Transaction Entity
    /// </summary>  
    [Table("stock_transaction")]
    public class StockTransactionEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// Stock id 
        /// </summary>
        [Column("stock_id")]
        public int StockId { get; set; }

        /// <summary>
        ///  qty (+) (-) based on transaction type
        /// </summary>
        [Column("quantity")]
        public decimal Quantity { get; set; } = 0;

        /// <summary>
        /// Sku Id 
        /// </summary>
        [Column("sku_id")]
        public int SkuId { get; set; }

        /// <summary>
        /// SkuOumID
        /// </summary>
        [Column("sku_uom_id")]
        public int? SkuUomId { get; set; }

        /// <summary>
        /// Current Conversion Rate
        /// </summary>
        [Column("current_conversion_rate")]
        public int? CurrentConversionRate { get; set; }

        /// <summary>
        /// Unit name
        /// </summary>
        [Column("unit_name")]
        public string? UnitName { get; set; }

        /// <summary>
        /// TransactionDate
        /// </summary>
        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        /// <summary>
        /// Transaction Type
        /// </summary>
        [Column("transaction_type")]
        public StockTransactionType TransactionType { get; set; }

        /// <summary>
        /// Suplier id 
        /// </summary>
        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        /// <summary>
        /// Supplier name 
        /// </summary>
        [Column("supplier_name")]
        public string? SupplierName { get; set; }

        /// <summary>
        /// Customer id
        /// </summary>
        [Column("customer_id")]
        public int? CustomerId { get; set; }


        /// <summary>
        /// Customer Name 
        /// </summary>
        [Column("customer_name")]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Ref receipt 
        /// </summary>
        [Column("ref_receipt")]
        public string? RefReceipt { get; set; }

        /// <summary>
        /// tenant id 
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; }
    }

    /// <summary>
    /// Specifies the type of stock transaction, such as inbound, outbound, transfer, or adjustment.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the nature of a stock movement or modification within
    /// inventory management operations. The values represent common transaction scenarios encountered in warehouse and
    /// inventory systems.</remarks>
    public enum StockTransactionType
    {
        /// <summary>
        /// Inbound: Stock coming into the warehouse, such as from suppliers or returns.
        /// </summary>
        Inbound,
        /// <summary>
        /// Outbound: Stock leaving the warehouse, such as for customer orders or transfers.
        /// </summary>
        Outbound,

    }
}
