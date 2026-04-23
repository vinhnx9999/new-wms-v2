using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums.Outbound;

namespace WMSSolution.WMS.Entities.Models.Dispatchlist
{
    /// <summary>
    /// Dispatch List Detail Entity
    /// </summary>
    [Table("dispatchlist_detail")]
    public class DispatchListDetailEntity : BaseModel
    {
        /// <summary>
        /// Dispatch list ID (FK)
        /// </summary>
        public int dispatchlist_id { get; set; }

        /// <summary>
        /// Navigation property to Dispatch list
        /// </summary>
        [ForeignKey("dispatchlist_id")]
        public virtual DispatchlistEntity? DispatchList { get; set; }

        /// <summary>
        /// SKU ID
        /// </summary>
        public int sku_id { get; set; }

        /// <summary>
        /// Unit of measurement ID (Base UOM)
        /// </summary>
        public int sku_uom_id { get; set; }

        /// <summary>
        /// Required quantity (Use Decimal for mixed warehouse)
        /// </summary>
        [Column(TypeName = "decimal(18, 4)")]
        public decimal req_qty { get; set; } = 0;

        /// <summary>
        /// Allocated quantity - Total sum(qty_pick) from Picklist table
        /// </summary>
        [Column(TypeName = "decimal(18, 4)")]
        public decimal allocated_qty { get; set; } = 0;

        /// <summary>
        /// Actual picked quantity - Total sum(qty_picked_actual) from Picklist table
        /// </summary>
        [Column(TypeName = "decimal(18, 4)")]
        public decimal picked_qty { get; set; } = 0;

        /// <summary>
        /// Line status (Pending -> Partial -> Completed)
        /// </summary>
        public DispatchDetailStatus status { get; set; } = DispatchDetailStatus.Pending;

        /// <summary>
        /// description
        /// </summary>
        public string? description { get; set; }

        /// <summary>
        /// List of locations (Pallets) to pick for this product line
        /// </summary>
        public virtual ICollection<DispatchpicklistEntity> PickLists { get; set; }
            = new List<DispatchpicklistEntity>();
    }
}
