namespace Wms.Theme.Web.Model.Pallet
{
    public class PalletPageSearchDTO
    {
        public int Id { get; set; }
        /// <summary>
        /// pallet Code 
        /// </summary>
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// create time 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// PalletStatus
        /// </summary>
        public int palletStatus { get; set; }
    }
}
