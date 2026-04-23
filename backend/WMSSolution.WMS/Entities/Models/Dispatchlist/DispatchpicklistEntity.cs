using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums.Outbound;
using WMSSolution.WMS.Entities.Models.Dispatchlist;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// dispatchpicklist  entity
    /// </summary>
    [Table("dispatchpicklist")]
    public class DispatchpicklistEntity : BaseModel
    {
        #region foreign table

        /// <summary>
        /// foreign table
        /// </summary>

        [ForeignKey("dispatchlist_id")]
        [Obsolete("Need to remove in soon.", false)]
        public DispatchlistEntity Dispatchlist { get; set; }

        #endregion foreign table

        #region Property

        /// <summary>
        /// dispatchlist_id
        /// </summary>
        [Obsolete("Need to remove in soon.", false)]
        public int dispatchlist_id { get; set; } = 0;

        /// <summary>
        /// detail
        /// </summary>
        public int? dispatch_detail_id { get; set; }

        /// <summary>
        /// fk to detail
        /// </summary>
        [ForeignKey("dispatch_detail_id")]
        public virtual DispatchListDetailEntity? DispatchDetail { get; set; }

        /// <summary>
        /// goods_owner_id
        /// </summary>
        public int goods_owner_id { get; set; } = 0;

        /// <summary>
        /// goods_location_id
        /// </summary>
        public int goods_location_id { get; set; } = 0;

        /// <summary>
        /// ref to pallet id
        /// </summary>
        public int? pallet_Id { get; set; }

        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// pick_qty
        /// </summary>
        public int pick_qty { get; set; } = 0;

        /// <summary>
        /// picked_qty
        /// </summary>
        public int picked_qty { get; set; } = 0;

        /// <summary>
        /// Decimal qty from stock 
        /// </summary>
        public decimal qty { get; set; }

        /// <summary>
        /// is_update_stock
        /// </summary>
        public bool is_update_stock { get; set; } = false;

        /// <summary>
        /// last_update_time
        /// </summary>
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// picker_id
        /// </summary>
        public int picker_id { get; set; } = 0;

        /// <summary>
        /// picker
        /// </summary>
        public string picker { get; set; } = string.Empty;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime expiry_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// price
        /// </summary>
        public decimal price { get; set; } = 0;

        /// <summary>
        /// putaway_date
        /// </summary>
        public DateTime putaway_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// status
        /// </summary>
        public DispatchPickListStatus status { get; set; } = DispatchPickListStatus.Pending;

        #endregion Property
    }
}