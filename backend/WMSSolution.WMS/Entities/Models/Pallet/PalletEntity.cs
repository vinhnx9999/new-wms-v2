using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.Models.Pallet
{
    /// <summary>
    /// pallet entity
    /// </summary>
    [Table("pallet")]
    public class PalletEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// Pallet Code
        /// </summary>
        public required string PalletCode { get; set; }

        /// <summary>
        /// represents the unique key of the pallet in the warehouse
        /// </summary>
        public string? PalletKey { get; set; }

        /// <summary>
        /// Pallet Status
        /// </summary>
        public PalletEnumStatus PalletStatus { get; set; } = PalletEnumStatus.Available;

        /// <summary>
        /// Check the pallet is full
        /// </summary>
        public bool IsFull { get; set; } = false;

        /// <summary>
        /// Check The pallet have many sku located in pallet or only once sku
        /// </summary>
        public bool IsMixed { get; set; } = true;

        /// <summary>
        /// MaxWeight of pallet can lift 
        /// </summary>
        public decimal? MaxWeight { get; set; }

        /// <summary>
        /// Current weight of Item located on pallet
        /// </summary>
        public decimal? CurrentWeight { get; set; }

        /// <summary>
        /// Lenght of pallet    
        /// </summary>
        public decimal? Length { get; set; }
        /// <summary>
        ///  width of pallet
        /// </summary>
        public decimal? Width { get; set; }

        /// <summary>
        /// Height of pallet
        /// </summary>
        public decimal? Height { get; set; }

        /// <summary>
        /// Type of pallet 
        /// wood ,plastic ,metal etc
        /// </summary>
        public string? PalletType { get; set; }

        /// <summary>
        /// Pallet Creation date
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant id
        /// </summary>
        public long TenantId { get; set; } = 1;
    }
}
